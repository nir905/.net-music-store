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

        public ActionResult ViewDisc(int id,int msgCode=-1)
        {
            switch (msgCode)
            {
                case 0:
                    ViewBag.msg = "Problem insert to cart";
                    ViewBag.msgType = "alert-danger";
                    break;
                case 1:
                    ViewBag.msg = "Added secessfuly to cart";
                    ViewBag.msgType = "alert-success";
                    break;
            }
            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();
                SQLiteCommand discCdCommand =
                    new SQLiteCommand(
                        @"select Disc.discID,Disc.name,Disc.artist,Category.categoryName,Disc.pictureUrl,Disc.price,added,duration,songsamount from Disc 
                                INNER JOIN Category on Category.categoryid = Disc.categoryid where discid=@discid",
                        m_dbConnection);
                discCdCommand.Parameters.Add(new SQLiteParameter("discid", id));
                using (SQLiteDataReader reader = discCdCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return View(new Disc()
                        {
                            DiscID = int.Parse(reader["discid"].ToString()),
                            Name = reader["name"].ToString(),
                            Artist = reader["artist"].ToString(),
                            Category = new Category() { CategoryName = reader["categoryname"].ToString() },
                            PictureUrl = reader["pictureurl"].ToString(),
                            Price = float.Parse(reader["price"].ToString()),
                            DiscAdded = Convert.ToDateTime(reader["added"].ToString()),
                            Duration = reader["duration"].ToString(),
                            SongsAmount = int.Parse(reader["songsamount"].ToString())
                        });
                    }
                }
            }
            return new HttpNotFoundResult("Disc Not Found");
        }
        [HttpPost]
        public ActionResult AddToCart(int DiscID, int number)
        {
            try
            {
                int LastDiscId = Convert.ToInt32((HttpContext.Request.Headers["Referer"].Split('/'))[5]);
                if (LastDiscId == DiscID)
                {
                    if(number<1 || number>10)
                        return RedirectToAction("ViewDisc", "Store",new { id= DiscID, msgCode = 0});

                    var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
                    using (var m_dbConnection = new SQLiteConnection(connectionString))
                    {
                        m_dbConnection.Open();
                        SQLiteCommand storeCdCommand = new SQLiteCommand(@"insert into Orders (userId,orderTime,discID,amount,isBought) values (@userid,datetime('now', 'localtime'),@discid,@amount,0)",m_dbConnection);
                        storeCdCommand.Parameters.Add(new SQLiteParameter("userid", ((User) Session["myUser"]).UserID));
                        storeCdCommand.Parameters.Add(new SQLiteParameter("discid", DiscID));
                        storeCdCommand.Parameters.Add(new SQLiteParameter("amount", number));
                        storeCdCommand.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex)
            {
                return RedirectToAction("ViewDisc", "Store" , new { id = DiscID, msgCode = 0 });
            }
            return RedirectToAction("ViewDisc", "Store", new { id = DiscID, msgCode = 1 });
        }
    }
}