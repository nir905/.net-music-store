using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Security.Application;

namespace Vladi2.Controllers
{
    public class ErrorController : BaseController
    {
        public ActionResult Index(string msg="")
        {
            ViewBag.msg = HttpUtility.UrlDecode(Sanitizer.GetSafeHtmlFragment(msg));
            return View();
        }
        public ActionResult Error404()
        {
            return View();
        }
    }
}