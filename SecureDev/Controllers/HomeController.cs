using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vladi2.Models;

namespace Vladi2.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            HttpCookie SID = HttpContext.Request.Cookies["SID"];
            if (SID==null || Session["myUser"] == null || SID.Value != HttpContext.Session.SessionID)
                return RedirectToAction("Index", "Login");
            return View();
        }
    }
}