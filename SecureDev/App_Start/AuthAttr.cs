using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Vladi2.App_Start
{
    public class AuthAttr : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpCookie SID = HttpContext.Current.Request.Cookies["SID"];
            if (SID == null || HttpContext.Current.Session["myUser"] == null || SID.Value != HttpContext.Current.Session.SessionID)
            {
                filterContext.Result = new RedirectResult("~/Login");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}