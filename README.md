# Fur.ExtensionPack
[![Build status](https://dev.azure.com/windylulu/Fur.ExtensionPack/_apis/build/status/Fur.ExtensionPack-CI)](https://dev.azure.com/windylulu/Fur.ExtensionPack/_build/latest?definitionId=2)
![Nuget](https://img.shields.io/nuget/v/Fur.ExtensionPack)
#### 介绍
针对[Fur](http://https://gitee.com/monksoul/Fur)基础功能的实用拓展包



#### 安装教程

1.  `nuget Install-Package Fur.ExtensionPack -Version 0.0.8`

#### 使用说明

# 1.  审计
对于大部分实体，都需要记录实体进入数据库的相关时间，以及操作人员等，目前此扩展包支持实体创建时间和修改时间的自动管理。
## 使用
将数据库上下文由继承于`AppDbContext`更改为继承于`AuditingDbContext`
```c#
    [AppDbContext("Sqlite3ConnectionString")]
    public class FurDbContext : AppDbContext<FurDbContext>
    {
        public FurDbContext(DbContextOptions<FurDbContext> options) : base(options)
        {

        }
        protected override void SavingChangesEvent(object sender, SavingChangesEventArgs e)
        {

        }

    }
```
然后，在需要自动管理时间的实体添加ICreationTime以及IUpdateTime接口
```c#
 public class Person : Entity,ICreationTime,IUpdateTime
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public Person()
        {
            CreatedTime = DateTime.Now;
            IsDeleted = false;
        }

        /// <summary>
        /// 姓名
        /// </summary>
        [MaxLength(32)]
        public string Name { get; set; }
        。。。
    }
```
这样就完成了，此实体新建的时候，将会自动更新创建时间，被更新的时候，写入更新时间，以及在更新的时候，防止创建时间被篡改。
# 2.修正Mapster的行为
Fur使用Mapster作为Map工具，这是一个非常强大且性能不错的库，然后此库有一些小瑕疵，当子实体列表一并被map作为更新的时候，Mapster是先清空列表，然后再插入列表，这样做是正常的，但是对于EF，它会认为这个子实体被完全修改了，从而为每个子实体生成了全量更新的Sql语句，这对于大型复杂实体来说，对性能的影响是致命性的。要解决这个问题的关键，是让Mapster知道，实体的哪个字段作为主键，并且根据主键去查找对应关系，应此本扩展包提供了一个`AdaptToTrack`拓展方法能够很方便地进行调用。
```c#
public async Task Update(PersonInputDto input)
{
  var person = await _personRepository.Entities.Include(u => u.PersonDetail).Include(u => u.Childrens).Include(u => u.Posts).SingleAsync(u => u.Id == input.Id.Value);
//直接Adapt，会清空Children列表然后插入，EF将会捕捉到所有Children被更新
// input.Adapt(person);
//AdaptToTrack会检测Children的主键(Id)的对应关系，来进行Map，EF只会捕捉到被修改过的更新
   input.AdaptToTrack(person);
   await _personRepository.UpdateAsync(person);

}
```
#### 参与贡献

1.  Fork 本仓库
2.  新建 Feat_xxx 分支
3.  提交代码
4.  新建 Pull Request


#### 特技

1.  使用 Readme\_XXX.md 来支持不同的语言，例如 Readme\_en.md, Readme\_zh.md
2.  Gitee 官方博客 [blog.gitee.com](https://blog.gitee.com)
3.  你可以 [https://gitee.com/explore](https://gitee.com/explore) 这个地址来了解 Gitee 上的优秀开源项目
4.  [GVP](https://gitee.com/gvp) 全称是 Gitee 最有价值开源项目，是综合评定出的优秀开源项目
5.  Gitee 官方提供的使用手册 [https://gitee.com/help](https://gitee.com/help)
6.  Gitee 封面人物是一档用来展示 Gitee 会员风采的栏目 [https://gitee.com/gitee-stars/](https://gitee.com/gitee-stars/)
