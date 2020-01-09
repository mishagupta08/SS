using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace InventoryManagement
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        
   //     protected void Application_Error()
   //{
   //   var ex = Server.GetLastError();
   //   //log the error!
   // //  _Logger.Error(ex);
   //         System.Diagnostics.Debug.WriteLine(ex);
   //         Response.Redirect("/Home/Error");
   //     }
    }
}
