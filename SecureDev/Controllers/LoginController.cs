using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vladi2.Models;

namespace Vladi2.Controllers
{
    public class LoginController : BaseController
    {
        public ActionResult Index()
        {
            HttpCookie SID = HttpContext.Request.Cookies["SID"];
            if (SID == null || Session["myUser"] == null || SID.Value != HttpContext.Session.SessionID)
                return View();
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public ActionResult Index(User user)
        {
            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();
                //TODO:add paramaters instead the SQL INJECTION
                using (SQLiteCommand command = new SQLiteCommand("SELECT * FROM Users Where userName = '" + user.UserName + "' and password = '" + user.Password+ "'", m_dbConnection))
                using (SQLiteDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        User myUser = new User()
                        {
                            UserID = reader.GetInt32(0),
                            FirstName = reader.GetString(1),
                            LastName = reader.GetString(2)
                        };
                        Session["myUser"] = myUser;
                        HttpContext.Response.SetCookie(new HttpCookie("SID", HttpContext.Session.SessionID));
                        return RedirectToAction("Index", "Home");
                    }
                }
            }
            ViewBag.ErrorMsg = "User name or Password is incorrect";
            return View();
        }
    }
}