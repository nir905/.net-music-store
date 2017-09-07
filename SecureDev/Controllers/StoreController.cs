using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Security.Application;
using Vladi2.App_Start;
using Vladi2.Helpers;
using Vladi2.Models;

namespace Vladi2.Controllers
{
    [AuthAttr]
    public class StoreController : BaseController
    {
        public ActionResult Index(string q = null)
        {
            q = Sanitizer.GetSafeHtmlFragment(q);
            ViewBag.Search = q;
            List<Disc> list = new List<Disc>();
            List<Category> categories = new List<Category>();
            try
            {
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
                                Category = new Category() {CategoryName = reader["categoryname"].ToString()},
                                PictureUrl = reader["pictureurl"].ToString(),
                                Price = float.Parse(reader["price"].ToString())
                            });
                        }
                    }

                    SQLiteCommand categoriesCommand = new SQLiteCommand(
                        "select categoryid, categoryName from Category", m_dbConnection);
                    using (SQLiteDataReader reader = categoriesCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new Category()
                            {
                                CategoryName = reader["categoryName"].ToString()
                            });
                        }
                    }
                    ViewBag.Categories = categories;
                }
                return View(list);
            }
            catch (SQLiteException)
            {
                Logger.WriteToLog(Logger.SQLLiteMsg);
                throw;
            }
            catch (Exception exception)
            {
                Logger.WriteToLog(exception);
                throw;
            }
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
            try
            {
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
                                Category = new Category() {CategoryName = reader["categoryname"].ToString()},
                                PictureUrl = reader["pictureurl"].ToString(),
                                Price = float.Parse(reader["price"].ToString()),
                                DiscAdded = Convert.ToDateTime(reader["added"].ToString()),
                                Duration = reader["duration"].ToString(),
                                SongsAmount = int.Parse(reader["songsamount"].ToString())
                            });
                        }
                    }
                }
                throw new HttpException(404,"Disc Not Found");
            }
            catch (SQLiteException)
            {
                Logger.WriteToLog(Logger.SQLLiteMsg);
                throw;
            }
            catch (Exception exception)
            {
                Logger.WriteToLog(exception);
                throw;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddToCart(int DiscID, int number)
        {
            try
            {
                int LastDiscId = Convert.ToInt32(HttpContext.Request.Headers["Referer"].Split('/')[5].Split('?')[0]);//get the disc id from referer
                if (LastDiscId == DiscID)
                {
                    if(number<1 || number>10)
                        return RedirectToAction("ViewDisc", "Store",new { id = DiscID, msgCode = 0});

                    var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
                    using (var m_dbConnection = new SQLiteConnection(connectionString))
                    {
                        m_dbConnection.Open();
                        SQLiteCommand storeCdCommand = new SQLiteCommand(@"insert into Orders (userId,orderTime,discID,amount,isBought) values (@userid,datetime('now', 'localtime'),@discid,@amount,0)",m_dbConnection);
                        storeCdCommand.Parameters.Add(new SQLiteParameter("userid", ((User) Session["myUser"]).UserID));
                        storeCdCommand.Parameters.Add(new SQLiteParameter("discid", DiscID));
                        storeCdCommand.Parameters.Add(new SQLiteParameter("amount", number));
                        storeCdCommand.ExecuteNonQuery();
                        return RedirectToAction("ViewDisc", "Store", new { id = DiscID, msgCode = 1 });
                    }
                }
                return RedirectToAction("ViewDisc", "Store", new { id = DiscID, msgCode = 0 });//Problem insert to cart
            }
            catch (SQLiteException)
            {
                Logger.WriteToLog(Logger.SQLLiteMsg);
                throw;
            }
            catch (Exception exception)
            {
                Logger.WriteToLog(exception);
                throw;
            }
        }
    }
}