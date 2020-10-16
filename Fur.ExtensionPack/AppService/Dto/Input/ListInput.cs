using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fur.ExtensionPack.AppService
{
    /// <summary>
    /// 列表查询参数
    /// </summary>
    public class ListInput : IListInput
    {
        /// <summary>
        /// 排序字段
        /// </summary>
        public string Order { get; set; }
        /// <summary>
        /// 筛选器
        /// </summary>
        public string Filter { get; set; }
        /// <summary>
        /// 跳过记录数
        /// </summary>
        public int? SkipCount { get; set; }
        /// <summary>
        /// 最大返回数量
        /// </summary>
        public int? MaxResultCount { get; set; }
    }
}
