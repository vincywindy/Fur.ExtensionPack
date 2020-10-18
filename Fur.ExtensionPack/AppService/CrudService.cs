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
using System.Linq.Dynamic.Core;
using Microsoft.AspNetCore.Mvc;
using Fur.ExtensionPack.AppService.Dto;

namespace Fur.ExtensionPack.AppService
{
    /// <summary>
    /// Crud服务
    /// </summary>
    /// <typeparam name="TDto">用于查询返回的Dto</typeparam>
    /// <typeparam name="TEntity">用于查询的实体</typeparam>
    /// <typeparam name="TCreateDto">用于调用创建接口的Dto</typeparam>
    /// <typeparam name="TUpdateDto">用于调用更新接口的Dto</typeparam>
    /// <typeparam name="TListInput">用于查询实体列表的参数</typeparam>
    /// <typeparam name="TKey">主键</typeparam>
    [SkipScan]
    public abstract class CrudService<TDto, TEntity, TCreateDto, TUpdateDto, TListInput, TKey>
        where TEntity : PrivateEntityBase<TKey>, new()
        where TDto : IDtoBase<TKey>
        where TListInput : IListInput
    {
        /// <summary>
        /// 删除时候是否使用软删除，默认清空下，如果实体带有软删除标记，那么就是软删除，如果没有，那就不是软删除
        /// </summary>
        protected bool IsFakeDelete;
        /// <summary>
        /// 是否计算返回数量
        /// </summary>
        protected bool IsCalculateCount;
        /// <summary>
        /// 默认使用的仓储
        /// </summary>
        protected readonly IRepository<TEntity> Repository;
        /// <summary>
        /// 初始化Service
        /// </summary>
        public CrudService()
        {
            Repository = Db.GetRepository<TEntity>();
            //设置软删除状态 主库有bug，所以先不做判断
            IsFakeDelete = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Any(u => u.IsDefined(typeof(FakeDeleteAttribute), true));
            IsCalculateCount = true;
        }
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="createDto"></param>
        /// <returns></returns>
        public async virtual Task<TDto> CreateAsync(TCreateDto createDto)
        {
            var newEntity = await Repository.InsertNowAsync(createDto.Adapt<TEntity>());
            return newEntity.Entity.Adapt<TDto>();
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
            var currentity = await CreateEntityQuery(Repository.Entities.Where(d => d.Id.Equals(inputentity.Id))).FirstAsync();
            var updatedentity = updateDto.AdaptToTrack(currentity);
            await Repository.SaveNowAsync();
            return updatedentity.Adapt<TDto>();
        }
        /// <summary>
        /// 获取一条记录
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async virtual Task<TDto> FindOne(TKey Id)
        {
            var currentity = await CreateEntityQuery(Repository.Entities.Where(d => d.Id.Equals(Id))).FirstAsync();
            return currentity.Adapt<TDto>();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async virtual Task<PageResultDto<TDto>> GetList([FromQuery] TListInput input)
        {
            var query = CreateFilterQuery(input.Filter);
            query = CreateListQuery(query);
            var page = new PageResultDto<TDto>();
            //先算数量后排序
            if (IsCalculateCount)
            {
                page.MaxCount = query.Count();
            }
            if (!string.IsNullOrEmpty(input.Order))
            {
                query = query.OrderBy(input.Order);
            }
            query = query.Skip(input.SkipCount);
            if (input.MaxResultCount == 0)
            {
                input.MaxResultCount = 10;
            }
            query = query.Take(input.MaxResultCount);
            page.Items = (await query.ToListAsync()).Adapt<List<TDto>>();
            return page;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async virtual Task DeleteAsync(TKey Id)
        {
            if (IsFakeDelete)
            {
                await Repository.FakeDeleteAsync(Id);
            }
            else
            {
                await Repository.DeleteAsync(Id);
            }

        }
        /// <summary>
        /// 取得单一实体的Query，被UpdateAsync和GetAsync调用
        /// </summary>
        /// <returns></returns>
        protected virtual IQueryable<TEntity> CreateEntityQuery(IQueryable<TEntity> query)
        {
            return query;
        }
        /// <summary>
        /// 取得筛选器的Query，默认Filter使用Dynamic.Core
        /// </summary>
        /// <param name="Filter"></param>
        /// <returns></returns>
        protected virtual IQueryable<TEntity> CreateFilterQuery(string Filter)
        {
            var query = Repository.DetachedEntities;
            if (!string.IsNullOrEmpty(Filter))
            {
                query = query.Where(Filter);
            }

            return query;
        }
        /// <summary>
        /// 取得实体列表的Query，被GetList调用
        /// </summary>
        /// <returns></returns>
        protected virtual IQueryable<TEntity> CreateListQuery(IQueryable<TEntity> query)
        {

            return query;

        }

    }
    /// <summary>
    /// Crud服务
    /// </summary>
    /// <typeparam name="TDto">用于查询返回以及进行修改的的Dto</typeparam>
    /// <typeparam name="TEntity">用于查询的实体</typeparam>
    /// <typeparam name="TCreateDto">用于调用创建接口的Dto</typeparam>
    /// <typeparam name="TListInput">用于查询实体列表的参数</typeparam>
    /// <typeparam name="TKey">实体的主键</typeparam>
    [SkipScan]
    public abstract class CrudService<TDto, TEntity, TCreateDto, TListInput, TKey> : CrudService<TDto, TEntity, TCreateDto, TDto, TListInput, TKey>
        where TEntity : PrivateEntityBase<TKey>, new()
        where TDto : IDtoBase<TKey>
        where TListInput : IListInput
    {

    }
    /// <summary>
    /// Crud服务
    /// </summary>
    /// <typeparam name="TDto">用于查询，创建，更新返回以及进行修改的的Dto</typeparam>
    /// <typeparam name="TEntity">用于查询的实体</typeparam>
    /// <typeparam name="TListInput">用于查询实体列表的参数</typeparam>
    /// <typeparam name="TKey">实体的主键</typeparam>
    [SkipScan]
    public abstract class CrudService<TDto, TEntity, TListInput, TKey> : CrudService<TDto, TEntity, TDto, TDto, TListInput, TKey>
        where TEntity : PrivateEntityBase<TKey>, new()
        where TDto : IDtoBase<TKey>
        where TListInput : IListInput
    {

    }
    /// <summary>
    /// Crud服务
    /// </summary>
    /// <typeparam name="TDto">用于查询，创建，更新返回以及进行修改的的Dto</typeparam>
    /// <typeparam name="TEntity">用于查询的实体</typeparam>
    /// <typeparam name="TKey">实体的主键</typeparam>
    [SkipScan]
    public abstract class CrudService<TDto, TEntity, TKey> : CrudService<TDto, TEntity, TDto, TDto, ListInput, TKey>
        where TEntity : PrivateEntityBase<TKey>, new()
        where TDto : IDtoBase<TKey>
    {

    }
    /// <summary>
    /// Crud服务
    /// </summary>
    /// <typeparam name="TDto">用于查询，创建，更新返回以及进行修改的的Dto</typeparam>
    /// <typeparam name="TEntity">用于查询的实体</typeparam>
    [SkipScan]
    public abstract class CrudService<TDto, TEntity> : CrudService<TDto, TEntity, TDto, TDto, ListInput, int>
        where TEntity : PrivateEntityBase<int>, new()
        where TDto : IDtoBase<int>
    {

    }
}
