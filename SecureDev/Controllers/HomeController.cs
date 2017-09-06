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
            ViewBag.FirstName = (Session["myUser"] as User).FirstName;
            List<Disc> list = new List<Disc>();
            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();
                using (SQLiteCommand lastCdCommand = new SQLiteCommand("select Disc.discID,Disc.name,Disc.artist,Category.categoryName,Disc.pictureUrl,Disc.price from Disc INNER JOIN Category on Category.categoryid = Disc.categoryid order by datetime(added, 'localtime') desc LIMIT 4", m_dbConnection))
                using (SQLiteDataReader reader = lastCdCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Disc()
                        {
                            DiscID = int.Parse(reader["discid"].ToString()),
                            Name =  reader["name"].ToString(),
                            Artist = reader["artist"].ToString(),
                            Category = new Category() { CategoryName = reader["categoryname"].ToString() },
                            PictureUrl = reader["pictureurl"].ToString(),
                            Price = float.Parse(reader["price"].ToString())
                        });
                    }
                }
            }
            return View(list);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            HttpContext.Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId") { Expires = DateTime.Now.AddDays(-1) });
            return RedirectToAction("Index", "Login");
        }
    }
}