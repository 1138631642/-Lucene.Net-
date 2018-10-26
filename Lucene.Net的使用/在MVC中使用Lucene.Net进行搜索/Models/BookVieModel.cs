using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace 在MVC中使用Lucene.Net进行搜索.Models
{
    public class BookVieModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public LuceneTypeEnum LuceneTypeEnum { get; set; }
    }
}