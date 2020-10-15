using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fur.ExtensionPack.Entity
{
    /// <summary>
    /// 标准用户
    /// </summary>
    public abstract class StandardUser<TPrimaryKey> : IdentityUser<TPrimaryKey>, ICreationTime, IUpdateTime where TPrimaryKey : IEquatable<TPrimaryKey>
    {
      
        
        /// <summary>
        /// 用户名
        /// </summary>
        public override string UserName { get => base.UserName; set => base.UserName = value; }
        /// <summary>
        /// 两部验证
        /// </summary>
        public override bool TwoFactorEnabled { get => base.TwoFactorEnabled; set => base.TwoFactorEnabled = value; }
        /// <summary>
        /// 安全码
        /// </summary>
        public override string SecurityStamp { get => base.SecurityStamp; set => base.SecurityStamp = value; }
        /// <summary>
        /// 确认电话
        /// </summary>
        public override bool PhoneNumberConfirmed { get => base.PhoneNumberConfirmed; set => base.PhoneNumberConfirmed = value; }
        /// <summary>
        /// 电话号码
        /// </summary>
        public override string PhoneNumber { get => base.PhoneNumber; set => base.PhoneNumber = value; }
        /// <summary>
        /// 密码哈希
        /// </summary>
        public override string PasswordHash { get => base.PasswordHash; set => base.PasswordHash = value; }
        /// <summary>
        /// 规范化用户名
        /// </summary>
        public override string NormalizedUserName { get => base.NormalizedUserName; set => base.NormalizedUserName = value; }
        /// <summary>
        /// 规范化邮箱
        /// </summary>
        public override string NormalizedEmail { get => base.NormalizedEmail; set => base.NormalizedEmail = value; }
        /// <summary>
        /// 解锁时间
        /// </summary>
        public override DateTimeOffset? LockoutEnd { get => base.LockoutEnd; set => base.LockoutEnd = value; }
        /// <summary>
        /// 是否被锁定
        /// </summary>
        public override bool LockoutEnabled { get => base.LockoutEnabled; set => base.LockoutEnabled = value; }
        /// <summary>
        /// 是否确认邮箱
        /// </summary>
        public override bool EmailConfirmed { get => base.EmailConfirmed; set => base.EmailConfirmed = value; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public override string Email { get => base.Email; set => base.Email = value; }
        /// <summary>
        /// 并发乐观锁
        /// </summary>
        [ConcurrencyCheck]
        public override string ConcurrencyStamp { get => base.ConcurrencyStamp; set => base.ConcurrencyStamp = value; }
        /// <summary>
        /// 登录失败计数
        /// </summary>
        public override int AccessFailedCount { get => base.AccessFailedCount; set => base.AccessFailedCount = value; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreatedTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public virtual DateTime? UpdatedTime { get; set; }
    }
}
