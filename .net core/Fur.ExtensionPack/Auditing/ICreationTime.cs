﻿
using Fur.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fur
{
    /// <summary>
    /// 创建时间字段
    /// </summary>
    public interface ICreationTime
    {
        /// <summary>
        /// 创建时间
        /// </summary>
        DateTime CreatedTime { get; set; }
    }
}
