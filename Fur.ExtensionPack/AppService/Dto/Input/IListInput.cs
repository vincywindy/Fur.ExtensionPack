using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fur.ExtensionPack.AppService
{
    /// <summary>
    /// 查询列表的输入参数接口
    /// </summary>
    public interface IListInput
    {
        /// <summary>
        /// 排序
        /// </summary>
        string Order { get; set; }
        /// <summary>
        /// 筛选条件
        /// </summary>
        string Filter { get; set; }
        /// <summary>
        /// 跳过记录数
        /// </summary>
        int? SkipCount { get; set; }
        /// <summary>
        /// 最大返回数量
        /// </summary>
        int? MaxResultCount { get; set; }
    }
}
