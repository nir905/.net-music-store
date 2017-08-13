using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vladi2.App_Start;
using Vladi2.Models;

namespace Vladi2.Controllers
{
    [AuthAttr]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Logout()
        {
            HttpContext.Response.Cookies.Add(new HttpCookie("SID") { Expires = DateTime.Now.AddDays(-1) });
            Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}