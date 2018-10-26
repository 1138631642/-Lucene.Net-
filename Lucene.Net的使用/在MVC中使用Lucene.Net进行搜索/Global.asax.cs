using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using 在MVC中使用Lucene.Net进行搜索.Models;

namespace 在MVC中使用Lucene.Net进行搜索
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            // 启动往lucene.net添加搜索词的线程
            IndexManager.GetInstance().StartThread();

            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}
