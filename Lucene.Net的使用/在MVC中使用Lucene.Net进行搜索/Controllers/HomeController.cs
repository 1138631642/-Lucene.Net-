using Lucene.Net.Analysis;
using Lucene.Net.Analysis.PanGu;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using PanGu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using 在MVC中使用Lucene.Net进行搜索.Models;

namespace 在MVC中使用Lucene.Net进行搜索.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            ItcastCmsEntities db = new ItcastCmsEntities();
            //var books = db.Books.ToList();
            //ViewBag.books = books;
            return View();
        }

        /// <summary>
        /// 进行搜索
        /// </summary>
        /// <returns></returns>
        public ActionResult Search()
        {
            string kw = Request["kw"]; // 获取用户输入的搜索内容
            string indexPath = Server.MapPath("~/lucenedir"); // 从哪里搜索

            // 对用户输入的内容进行分割
            List<string> kws = new List<string>();  // 定义一个集合用来存储分割后的分词
            Analyzer analyzer = new PanGuAnalyzer();
            TokenStream tokenStream = analyzer.TokenStream("", new StringReader(kw.ToString()));
            Lucene.Net.Analysis.Token token = null;
            while ((token = tokenStream.Next()) != null)
            {
                kws.Add(token.TermText());
            }

            FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexPath), new NoLockFactory());
            IndexReader reader = IndexReader.Open(directory, true);
            IndexSearcher searcher = new IndexSearcher(reader);
            //搜索条件

            // 注意：这个类只可以进行单个列条件搜索，如果想要实现多个条件搜索要使用另外一个类
            PhraseQuery query = new PhraseQuery();
            foreach (var word in kws)
            {
                query.Add(new Term("content", word)); // 向content这个列进行搜索
            }

            query.SetSlop(100);//多个查询条件的词之间的最大距离.在文章中相隔太远 也就无意义.（例如 “大学生”这个查询条件和"简历"这个查询条件之间如果间隔的词太多也就没有意义了。）
            //TopScoreDocCollector是盛放查询结果的容器
            TopScoreDocCollector collector = TopScoreDocCollector.create(1000, true);
            searcher.Search(query, null, collector);//根据query查询条件进行查询，查询结果放入collector容器
            ScoreDoc[] docs = collector.TopDocs(0, collector.GetTotalHits()).scoreDocs;//得到所有查询结果中的文档,GetTotalHits():表示总条数   TopDocs(300, 20);//表示得到300（从300开始），到320（结束）的文档内容.

            // 创建一个list集合用来存储搜索到的结果
            List<BookVieModel> bookList = new List<BookVieModel>();
            for (int i = 0; i < docs.Length; i++)
            {
               

                //搜索ScoreDoc[]只能获得文档的id,这样不会把查询结果的Document一次性加载到内存中。降低了内存压力，需要获得文档的详细内容的时候通过searcher.Doc来根据文档id来获得文档的详细内容对象Document.
                int docId = docs[i].doc;//得到查询结果文档的id（Lucene内部分配的id）
                Document doc = searcher.Doc(docId);//找到文档id对应的文档详细信息

                BookVieModel model = new BookVieModel();
                model.Id = Convert.ToInt32(doc.Get("Id")); // 注意：这些字段要和在添加搜索词库的时候保持一致
                model.Title = CreateHightLight(kw,doc.Get("title")); // 注意：这些字段要和在添加搜索词库的时候保持一致
                // 对搜索到结果中的搜索词进行高亮显示
                model.Content = CreateHightLight(kw,doc.Get("content")); // 注意：这些字段要和在添加搜索词库的时候保持一致

                bookList.Add(model);
            }
            ViewBag.books = bookList;
            ViewBag.kw = kw;
            return View("Index");
        }

        /// <summary>
        /// 创建搜索库(在初始化搜索词库的时候使用)
        /// </summary>
        /// <returns></returns>
        public ActionResult GetSearchData()
        {
            string indexPath = Server.MapPath("~/lucenedir");//@"C:\Users\杨ShineLon\Desktop\lucenedir";//注意和磁盘上文件夹的大小写一致，否则会报错。将创建的分词内容放在该目录下。
            FSDirectory directory = FSDirectory.Open(new DirectoryInfo(indexPath), new NativeFSLockFactory());//指定索引文件(打开索引目录) FS指的是就是FileSystem
            bool isUpdate = IndexReader.IndexExists(directory);//IndexReader:对索引进行读取的类。该语句的作用：判断索引库文件夹是否存在以及索引特征文件是否存在。
            if (isUpdate)
            {
                //同时只能有一段代码对索引库进行写操作。当使用IndexWriter打开directory时会自动对索引库文件上锁。
                //如果索引目录被锁定（比如索引过程中程序异常退出），则首先解锁（提示一下：如果我现在正在写着已经加锁了，但是还没有写完，这时候又来一个请求，那么不就解锁了吗？这个问题后面会解决）
                if (IndexWriter.IsLocked(directory))
                {
                    IndexWriter.Unlock(directory);
                }
            }
            IndexWriter writer = new IndexWriter(directory, new PanGuAnalyzer(), !isUpdate, Lucene.Net.Index.IndexWriter.MaxFieldLength.UNLIMITED);//向索引库中写索引。这时在这里加锁。

            // 将数据库中的数据遍历添加到搜索库中
            ItcastCmsEntities db = new ItcastCmsEntities();
            List<Books> books = db.Books.ToList();
            foreach (var book in books)
            {
                Document document = new Document();//表示一篇文档。

                //Field.Store.YES:表示是否存储原值。只有当Field.Store.YES在后面才能用doc.Get("number")取出值来.Field.Index. NOT_ANALYZED:不进行分词保存
                // 向文档中添加列。需要哪些字段就添加那些字段
                document.Add(new Field("Id", book.Id.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
                document.Add(new Field("title", book.Title, Field.Store.YES, Field.Index.ANALYZED, Lucene.Net.Documents.Field.TermVector.WITH_POSITIONS_OFFSETS));
                document.Add(new Field("content", book.ContentDescription, Field.Store.YES, Field.Index.ANALYZED, Lucene.Net.Documents.Field.TermVector.WITH_POSITIONS_OFFSETS));

                writer.AddDocument(document);
            }

            writer.Close();//会自动解锁。
            directory.Close();//不要忘了Close，否则索引结果搜不到

            return Content("ok");
        }

        /// <summary>
        /// 进行高亮显示搜索词
        /// </summary>
        /// <param name="keywords"></param>
        /// <param name="Content"></param>
        /// <returns></returns>
        public static string CreateHightLight(string keywords, string Content)
        {
            // 注意：在后台加 HTML标签会导致前台原样输出，可以通过
            // @MvcHtmlString.Create(book.Content)来解决
            PanGu.HighLight.SimpleHTMLFormatter simpleHTMLFormatter =
             new PanGu.HighLight.SimpleHTMLFormatter("<font color=\"red\">", "</font>");
            //创建Highlighter ，输入HTMLFormatter 和盘古分词对象Semgent
            PanGu.HighLight.Highlighter highlighter =
            new PanGu.HighLight.Highlighter(simpleHTMLFormatter,
            new Segment());
            //设置每个摘要段的字符数
            highlighter.FragmentSize = 150;
            //获取最匹配的摘要段
            return highlighter.GetBestFragment(keywords, Content);

        }

        /// <summary>
        /// 往lucene.net中添加数据
        /// </summary>
        /// <returns></returns>
        public ActionResult AddDataToLucene()
        {
            Books model = new Books();
            model.AurhorDescription = "jlkfdjf";
            model.Author = "asfasd";
            model.CategoryId = 1;
            model.Clicks = 1;
            model.ContentDescription = "Ajax高级编程";
            model.EditorComment = "adfsadfsadf";
            model.ISBN = "111111111111111111";
            model.PublishDate = DateTime.Now;
            model.PublisherId = 72;
            model.Title = "Ajax";
            model.TOC = "aaaaaaaaaaaaaaaa";
            model.UnitPrice = 22.3m;
            model.WordsCount = 1234;
            //1.将数据先存储到数据库中。获取刚插入的数据的主键ID值。
            IndexManager.GetInstance().AddQueue(9999, model.Title, model.ContentDescription);//向队列中添加
            return View("Index");
        }
        /// <summary>
        /// 往lucene.net中删除数据
        /// </summary>
        public ActionResult DeleteDataToLucene()
        {
            // 要先删除数据库中，然后再删lucene.net中的
            IndexManager.GetInstance().DeleteQueue(9999);
            return View("Index");
        }
    }
}