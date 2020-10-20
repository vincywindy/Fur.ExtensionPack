using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fur.ExtensionPack.AppService.Dto
{
    /// <summary>
    /// 用于返回列表的Dto
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    public class PageResultDto<TDto>
    {
        /// <summary>
        /// 查找到的记录总数
        /// </summary>
        public virtual int? MaxCount { get; set; }
        /// <summary>
        /// 对应记录
        /// </summary>
        public virtual List<TDto> Items { get; set; }
    }




}
