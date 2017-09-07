using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Microsoft.Security.Application;
using Vladi2.Helpers;
using Vladi2.Models;

namespace Vladi2.Controllers
{
    public class LoginController : BaseController
    {
        public ActionResult Index()
        {
            if (Session["myUser"] == null)
            {
                HttpContext.Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId") { Expires = DateTime.Now.AddDays(-1) });
                return View();
            }
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(User user)
        {
            try
            {
                UserResult result = user.Login();
                if(result.Status==UserResult.Statuses.Success)
                    return RedirectToAction("Index", "Home");
                ViewBag.ErrorMsg = result;
                return View();
            }
            catch (Exception exception)
            {
                Logger.WriteToLog(exception);
                throw;
            }
        }

    }
}