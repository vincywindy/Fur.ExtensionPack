using Fur.DatabaseAccessor;
using Mapster;
using Mapster.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Fur
{
    /// <summary>
    /// Mapper的拓展包
    /// </summary>
    public static class TypeAdapterBuilderExtensions
    {
        /// <summary>
        /// MapperDto到已被追踪的实体
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static TDestination AdaptToTrack<TSource, TDestination>(this TSource source, TDestination destination) where TDestination : class, IPrivateEntity, new()
        {
            var context = Db.GetRepository<TDestination>().DbContext;
            return source.BuildAdapter().EntityFromContext(context).AdaptTo(destination);
        }
        /// <summary>
        /// 根据数据库查询实体的关系，以进行Mapper
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="builder"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static TypeAdapterBuilder<TSource> EntityFromContext2<TSource>(this TypeAdapterBuilder<TSource> builder, DbContext context)
        {
            //保存配置参数名，当已经初始化过之后不会再重复执行Config的方法
            const string dbKey = "Mapster.EFCore.db";
            return builder
                .AddParameters(dbKey, context)
                //此过程只会执行一次，不会多次执行，应此不会有性能问题
                .ForkConfig(config =>
                {
                    //遍历Dbcontext查找所有实体
                    foreach (var entityType in context.Model.GetEntityTypes())
                    {
                        var type = entityType.ClrType;
                        //查找实体的主键
                        var keys = entityType.FindPrimaryKey()?.Properties.Select(p => p.Name).ToArray();
                        //如果实体没有主键，那就跳过，有可能是视图，或者其他奇怪的东西
                        if (keys == null)
                            continue;
                        //如果当前Map的对象实体=Dbcontext当前循环到的实体，则执行下列生成操作
                        var settings = config.When((srcType, destType, mapType) => destType == type);
                        settings.Settings.ConstructUsingFactory = arg =>
                        {
                            //指定参数，表示Dto
                            //$var1
                            var src = Expression.Parameter(arg.SourceType);
                            //判断调用的是xx.AdaptTo()还是xx.AdaptTo(aa),前者表示Map到一个新的对象，这个不需要我们进行处理，后者表示Map到已有的一个实体,然后指定参数，表示对应实体
                            //$var2
                            var dest = arg.MapType == MapType.MapToTarget
                                ? Expression.Parameter(arg.DestinationType)
                                : null;
                            //声明变量db，表示DbContext
                            //$db
                            var db = Expression.Variable(typeof(DbContext), "db");
                            //声明变量Set,表示实体的DbSet
                            //$set
                            var set = Expression.Variable(typeof(DbSet<>).MakeGenericType(arg.DestinationType), "set");
                            //取得当前Map的上下文
                            var current = typeof(MapContext).GetProperty(nameof(MapContext.Current), BindingFlags.Static | BindingFlags.Public);
                            //反射取到Dictionary<string, object>的Item
                            var indexer = typeof(Dictionary<string, object>).GetProperties().First(item => item.GetIndexParameters().Length > 0);
                            //生成Lambda表达式： $db = (Microsoft.EntityFrameworkCore.DbContext)((Mapster.MapContext.Current).Parameters).Item["Mapster.EFCore.db"]
                            var dbAssign = Expression.Assign(
                                db,
                                Expression.Convert(
                                    Expression.Property(
                                        Expression.Property(
                                            Expression.Property(null, current),
                                            nameof(MapContext.Parameters)),
                                        indexer,
                                        Expression.Constant(dbKey)),
                                    typeof(DbContext)));
                            //取得DbContext.Set(实体)的方法
                            var setMethod = (from method in typeof(DbContext).GetMethods()
                                             where method.Name == nameof(DbContext.Set) &&
                                                   method.IsGenericMethod
                                             select method).First().MakeGenericMethod(arg.DestinationType);
                            //调用DbContext.Set(实体)
                            //$set = .Call $db.Set()
                            var setAssign = Expression.Assign(set, Expression.Call(db, setMethod));
                            var getters =
                            //找到Map实体对应的主键字段(Id)
                            keys.Select(key => arg.DestinationType.GetProperty(key))
                            //New一个用于Mapster的PropertyModel，这里保存主键字段的对应规则
                                .Select(prop => new PropertyModel(prop))
                                //从已经声明的Map规则中，找到当前正在调用Dto与实体的对应规则
                                .Select(model => arg.Settings.ValueAccessingStrategies
                                    .Select(s => s(src, model, arg))
                                    .FirstOrDefault(exp => exp != null))
                                //查找表达式
                                //.Lambda #Lambda1<System.Func`3[Fur.Application.Persons.PersonInputDto,Fur.Core.Person,Fur.Core.Person]>(
                                //Fur.Application.Persons.PersonInputDto $var1,
                                //Fur.Core.Person $var2) {
                                //.Block(
                                // Microsoft.EntityFrameworkCore.DbContext $db,
                                // Microsoft.EntityFrameworkCore.DbSet`1[Fur.Core.Person] $set) {
                                // $db = (Microsoft.EntityFrameworkCore.DbContext)((Mapster.MapContext.Current).Parameters).Item["Mapster.EFCore.db"];
                                // $set = .Call $db.Set();
                                //           (.Call $set.Find(.NewArray System.Object[] {
                                //             (System.Object)$var1.Id
                                //           }) ?? $var2) ?? .New Fur.Core.Person()
                                // }
                                //      }
                                .Where(exp => exp != null)
                                //转换为Object
                                .Select(exp => Expression.Convert(exp, typeof(object)))
                                .ToArray();
                            //判断找到的对应的Id规则数量和主键数量是否相等，不相等说明不是需要处理的对象，不相等的场合比如没有主键Id的Dto，或者具有复合主键的实体等
                            if (getters.Length != keys.Length)
                                return null;
                            //生成Dto查询实体对应ID的表达式，
                            //.Call $set.Find(.NewArray System.Object[] {
                            //   (System.Object)$var1.Id
                            //}) 
                            Expression find = Expression.Call(set, nameof(DbContext.Find), null,
                                Expression.NewArrayInit(typeof(object), getters));
                            //判断是不是Map到对象实体
                            if (arg.MapType == MapType.MapToTarget)
                                //合并表达式，追加?? $var2
                                find = Expression.Coalesce(find, dest);
                            //(.Call $set.Find(.NewArray System.Object[] {
                            //(System.Object)$var1.Id
                            //}) ?? $var2) ?? .New Fur.Core.Person()
                            //构建.New Fur.Core.Person()
                            var ret = Expression.Coalesce(
                                find,
                                Expression.New(arg.DestinationType));
                            //以上，构建了一个表达式，Dto用找到的主键和实体的主键进行查找，如果查找到了那么就Map到对应的实体，否则将new一个和新的实体
                            //这是表达式的完全体：
                            //(.Call $set.Find(.NewArray System.Object[] {
                            //(System.Object)$var1.Id
                            //}) ?? $var2) ?? .New Fur.Core.Person()
                            //}
                            //把表达式注入对应的参数
                            var exp = Expression.Lambda(
                                                    Expression.Block(new[] { db, set }, dbAssign, setAssign, ret),
                                                    arg.MapType == MapType.MapToTarget ? new[] { src, dest } : new[] { src });
                            //这是生成的方法
                            //                            .Block(
                            //    Microsoft.EntityFrameworkCore.DbContext $db,
                            //    Microsoft.EntityFrameworkCore.DbSet`1[Fur.Core.Person] $set) {
                            //    $db = (Microsoft.EntityFrameworkCore.DbContext)((Mapster.MapContext.Current).Parameters).Item["Mapster.EFCore.db"];
                            //    $set = .Call $db.Set();
                            //                            (.Call $set.Find(.NewArray System.Object[] {
                            //                                (System.Object)$var1.Id
                            //                                }) ?? $var2) ?? .New Fur.Core.Person()
                            //}
                            return exp;
                        };
                    }
                }, context.GetType().FullName);
        }
    }
}
