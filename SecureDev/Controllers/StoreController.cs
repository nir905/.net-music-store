using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Security.Application;
using Vladi2.App_Start;
using Vladi2.Models;

namespace Vladi2.Controllers
{
    [AuthAttr]
    public class StoreController : BaseController
    {
        // GET: Store
        public ActionResult Index(string q = null)
        {
            q = Sanitizer.GetSafeHtmlFragment(q);
            ViewBag.Search = q;
            List<Disc> list = new List<Disc>();
            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();
                SQLiteCommand storeCdCommand;
                if (q == null)
                    storeCdCommand =
                        new SQLiteCommand(
                            @"select Disc.discID,Disc.name,Disc.artist,Category.categoryName,Disc.pictureUrl,Disc.price from Disc 
                                INNER JOIN Category on Category.categoryid = Disc.categoryid order by datetime(added, 'localtime') desc",
                            m_dbConnection);
                else
                {
                    storeCdCommand =
                        new SQLiteCommand(
                            @"select Disc.discID,Disc.name,Disc.artist,Category.categoryName,Disc.pictureUrl,Disc.price from Disc
                                INNER JOIN Category on Category.categoryid = Disc.categoryid
                                where name like '%'||@q||'%' or artist like '%'||@q||'%' or categoryname like '%'||@q||'%'
                                order by datetime(added, 'localtime') desc",
                            m_dbConnection);
                    storeCdCommand.Parameters.Add(new SQLiteParameter("q", q));
                }
                using (SQLiteDataReader reader = storeCdCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Disc()
                        {
                            DiscID = int.Parse(reader["discid"].ToString()),
                            Name = reader["name"].ToString(),
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
    }
}