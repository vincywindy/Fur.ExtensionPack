
using Fur.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fur
{
    /// <summary>
    /// 创建更新时间字段，更新时间在更新的时候由系统自动更新
    /// </summary>

    public interface IUpdateTime
    {
        /// <summary>
        /// 更新时间
        /// </summary>
        DateTime? UpdatedTime { get; set; }
    }
}
