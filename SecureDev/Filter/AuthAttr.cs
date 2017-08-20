using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vladi2.Models;

namespace Vladi2.App_Start
{
    public class AuthAttr : ActionFilterAttribute
    {
        public bool OnlyAdmin { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (HttpContext.Current.Session["myUser"] == null)//not connected
            {
                filterContext.Result = new RedirectResult("~/Login");
                return;
            }
            if (OnlyAdmin && !(HttpContext.Current.Session["myUser"] as User).IsAdmin)//not connected as admin
            {
                filterContext.Result = new RedirectResult("~/Home");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}