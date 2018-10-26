using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace 在MVC中使用Lucene.Net进行搜索.Models
{
    /// <summary>
    /// 用于表示队列中是添加还是删除
    /// </summary>
    public enum LuceneTypeEnum
    {
        /// <summary>
        /// 添加
        /// </summary>
        Add,
        /// <summary>
        /// 删除
        /// </summary>
        Delete
    }
}