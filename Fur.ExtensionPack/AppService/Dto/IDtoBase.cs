using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fur.ExtensionPack.AppService.Dto
{
    /// <summary>
    /// Dto的基类的接口
    /// </summary>
    /// <typeparam name="Key"></typeparam>
    public interface IDtoBase<Key>
    {
        /// <summary>
        /// 对应实体的主键ID
        /// </summary>
        Key Id { get; set; }
    }
}
