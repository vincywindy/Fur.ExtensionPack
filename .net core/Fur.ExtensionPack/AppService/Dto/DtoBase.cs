using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fur.ExtensionPack.AppService.Dto
{
    /// <summary>
    /// Dto的基础实现
    /// </summary>
    public class DtoBase : IDtoBase<int>
    {
        /// <summary>
        /// 对应实体的主键
        /// </summary>
        public int Id { get; set; }
    }
}
