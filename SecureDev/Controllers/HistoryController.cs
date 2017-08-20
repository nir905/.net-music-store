using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vladi2.App_Start;
using Vladi2.Models;

namespace Vladi2.Controllers
{
    [AuthAttr]
    public class HistoryController : BaseController
    {
        public ActionResult Index()
        {
            float totalPrice = 0;
            List<Order> list = new List<Order>();

            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();
                using (SQLiteCommand OrdersCommand = new SQLiteCommand(@"select Disc.discID,Disc.name,Disc.artist,Category.categoryName,Disc.pictureUrl,Disc.price,added,duration,songsamount,Orders.amount,Orders.orderTime,Orders.orderID,Orders.address,Orders.city,Orders.country from Disc 
                                INNER JOIN Category on Category.categoryid = Disc.categoryid
								INNER JOIN Orders on Orders.discID = Disc.discID
								where Orders.userId=@userid and Orders.isBought=1 order by orderid", m_dbConnection))
                {
                    OrdersCommand.Parameters.Add(new SQLiteParameter("userID", ((User)Session["myUser"]).UserID));

                    using (SQLiteDataReader reader = OrdersCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Order()
                            {
                                Disc = new Disc()
                                {
                                    DiscID = int.Parse(reader["discid"].ToString()),
                                    Name = reader["name"].ToString(),
                                    Artist = reader["artist"].ToString(),
                                    Category = new Category() { CategoryName = reader["categoryname"].ToString() },
                                    PictureUrl = reader["pictureurl"].ToString(),
                                    Price = float.Parse(reader["price"].ToString())
                                },
                                Amount = int.Parse(reader["amount"].ToString()),
                                OrderID = int.Parse(reader["orderid"].ToString()),
                                Address = reader["address"].ToString(),
                                City = reader["city"].ToString(),
                                Country = reader["country"].ToString()
                            });
                            totalPrice += (float.Parse(reader["price"].ToString())) *
                                          (int.Parse(reader["amount"].ToString()));
                        }
                    }
                }
            }
            ViewBag.TotalPrice = totalPrice;
            return View(list);
        }
    }
}