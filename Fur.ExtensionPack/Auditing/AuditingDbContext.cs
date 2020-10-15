using Fur.DatabaseAccessor;
using Fur.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fur.Auditing
{
    /// <summary>
    /// 附带审计功能的数据库上下文
    /// </summary>
    /// <typeparam name="TDbContext"></typeparam>
    [SkipScan]
    public abstract class AuditingDbContext<TDbContext> : AppDbContext<TDbContext> where TDbContext : DbContext
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="options"></param>
        public AuditingDbContext(DbContextOptions<TDbContext> options) : base(options)
        {
        }
        /// <summary>
        /// 保存的时候，自动管理创建时间以及更新时间
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void SavingChangesEvent(object sender, SavingChangesEventArgs e)
        {
            var context = sender as TDbContext;
            // 获取所有新增和更新的实体
            var entities = context.ChangeTracker.Entries().Where(u => u.State == EntityState.Added || u.State == EntityState.Modified || u.State == EntityState.Deleted).ToList();
            //子实体被修改的时候，去这里找旧的值，这样就不用去数据库再找一遍了
            var deletedcache = entities.Where(d => d.State == EntityState.Deleted).ToDictionary(d => d.Entity.GetType().FullName + d.Property("Id").CurrentValue, d => d);
            foreach (var entity in entities)
            {
                switch (entity.State)
                {
                    case EntityState.Added:
                        if (entity.Entity is not ICreationTime creationtimeentity)
                        {
                            continue;
                        }
                        if (creationtimeentity.CreatedTime == default)
                        {
                            creationtimeentity.CreatedTime = DateTime.Now;
                        }
                        break;
                    case EntityState.Modified:
                        if (entity.Entity is ICreationTime)
                        {
                            var key = entity.Entity.GetType().FullName + entity.Property("Id").CurrentValue;
                            if (deletedcache.ContainsKey(key))
                            {
                                entity.Property(nameof(ICreationTime.CreatedTime)).CurrentValue = deletedcache[key].Property(nameof(ICreationTime.CreatedTime)).CurrentValue;
                            }
                            else
                            {
                                entity.Property(nameof(ICreationTime.CreatedTime)).IsModified = false;
                            }

                        }
                        if (entity.Entity is IUpdateTime updatetimeentity)
                        {

                            updatetimeentity.UpdatedTime = DateTime.Now;
                        }
                        break;
                }
            }
            base.SavingChangesEvent(sender, e);
        }
    }
}
