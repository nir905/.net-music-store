using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Vladi2.Models;

namespace Vladi2
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

        void Application_Error(Object sender, EventArgs e)
        {
            Exception exception = Server.GetLastError();//Can user Logger
            string msg = (Session["myUser"] != null && (Session["myUser"] as User).IsAdmin) ? exception.Message : "";//for admin only!
            HttpException httpException = exception as HttpException;
            Server.ClearError();
            if (httpException != null)
            {
                string action;
                switch (httpException.GetHttpCode())
                {
                    case 404:
                        // page not found
                        action = "Error404";
                        break;
                    case 500:
                        // server error
                        action = "";
                        break;
                    default:
                        action = "";
                        break;
                }
                Response.Redirect(String.Format(@"~/Error/{0}?msg={1}", action, HttpUtility.UrlEncode(msg)));
            }
            else
            {
                Response.Redirect(String.Format(@"~/Error/Index?msg={0}", HttpUtility.UrlEncode(msg)));
            }
        }
    }
}
