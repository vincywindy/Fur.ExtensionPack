using Fur.DatabaseAccessor;
using Fur.DependencyInjection;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fur;
using System.Reflection;

namespace Fur.ExtensionPack.AppService
{
    /// <summary>
    /// Crud服务
    /// </summary>
    /// <typeparam name="TDto">用于查询返回的Dto</typeparam>
    /// <typeparam name="TEntity">用于查询的租户</typeparam>
    /// <typeparam name="TCreateDto">用于调用创建接口的Dto</typeparam>
    /// <typeparam name="TUpdateDto">用于调用更新接口的Dto</typeparam>
    /// <typeparam name="TListInput">用于查询实体列表的参数</typeparam>
    /// <typeparam name="TKey">主键</typeparam>
    [SkipScan]
    public abstract class CrudService<TDto, TEntity, TCreateDto, TUpdateDto, TListInput, TKey>
        where TEntity : PrivateEntityBase<TKey>, new()
        where TKey : struct
        where TListInput : IListInput
    {
        /// <summary>
        /// 删除时候是否使用软删除，默认清空下，如果实体带有软删除标记，那么就是软删除，如果没有，那就不是软删除
        /// </summary>
        public bool IsFakeDelete;
        /// <summary>
        /// 默认使用的仓储
        /// </summary>
        public readonly IRepository<TEntity> Repository;
        /// <summary>
        /// 初始化Service
        /// </summary>
        public CrudService()
        {
            Repository = Db.GetRepository<TEntity>();
            //设置软删除状态
            IsFakeDelete = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Any(u => u.IsDefined(typeof(FakeDeleteAttribute), true));
        }
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="createDto"></param>
        /// <returns></returns>
        public async virtual Task<TDto> CreateAsync(TCreateDto createDto)
        {
            var newEntity = await Repository.InsertNowAsync(createDto.Adapt<TEntity>());
            return newEntity.Adapt<TDto>();
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="updateDto"></param>
        /// <returns></returns>
        public async virtual Task<TDto> UpdateAsync(TUpdateDto updateDto)
        {
            var inputentity = updateDto.Adapt<TEntity>();
            //为了防止query里面需要load，应此先把条件加上
            var currentity = await GetEntityQuery(Repository.Entities.Where(d => d.Id.Equals(inputentity.Id))).FirstAsync();
            var updatedentity = inputentity.AdaptToTrack(currentity);
            await Repository.SaveNowAsync();
            return updatedentity.Adapt<TDto>();
        }
        public async virtual Task GetList(IListInput input)
        {

        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async virtual Task DeleteAsync(TKey Id)
        {
            await Repository.FakeDeleteAsync(Id);
        }
        /// <summary>
        /// 取得单一实体的Query，被UpdateAsync和GetAsync调用
        /// </summary>
        /// <returns></returns>
        protected virtual IQueryable<TEntity> GetEntityQuery(IQueryable<TEntity> query)
        {
            return query;
        }
        /// <summary>
        /// 取得实体列表的Query，被GetList调用
        /// </summary>
        /// <returns></returns>
        protected virtual IQueryable<TEntity> GetListQuery(IQueryable<TEntity> query)
        {
            return query;
        }
    }
    /// <summary>
    /// Crud服务
    /// </summary>
    /// <typeparam name="TDto">用于查询返回以及进行修改的的Dto</typeparam>
    /// <typeparam name="TEntity">用于查询的租户</typeparam>
    /// <typeparam name="TCreateDto">用于调用创建接口的Dto</typeparam>
    /// <typeparam name="TListInput">用于查询实体列表的参数</typeparam>
    /// <typeparam name="TKey">实体的主键</typeparam>
    [SkipScan]
    public abstract class CrudService<TDto, TEntity, TCreateDto, TListInput, TKey> : CrudService<TDto, TEntity, TCreateDto, TDto, TListInput, TKey>
        where TEntity : PrivateEntityBase<TKey>, new()
        where TKey : struct
        where TListInput : IListInput
    {

    }
}
