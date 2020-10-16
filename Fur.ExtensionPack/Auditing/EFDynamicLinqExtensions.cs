using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;

namespace Fur
{
    /// <summary>
    /// 参数化查询配置
    /// </summary>
    public static class EFDynamicLinqExtensions
    {
        /// <summary>
        /// 参数化查询配置
        /// </summary>
        public static ParsingConfig DefaultConfig;
        static EFDynamicLinqExtensions()
        {
            DefaultConfig = new ParsingConfig()
            {
                UseParameterizedNamesInDynamicQuery = true
            };
        }
        /// <summary>
        /// Where优化版
        /// </summary>
        /// <param name="source"></param>
        /// <param name="predicate"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IQueryable<T> Where<T>([NotNull] this IQueryable<T> source, [NotNull] string predicate, params object[] args)
        {
            return source.Where(DefaultConfig, predicate, args);
        }
    }
}
