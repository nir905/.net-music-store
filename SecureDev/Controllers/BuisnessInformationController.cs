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
    public class BuisnessInformationController : BaseController
    {
        public ActionResult Index()
        {
            List<Order> list = new List<Order>();

            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();
                using (SQLiteCommand OrdersCommand = new SQLiteCommand(@"Select Disc.name, Disc.artist, Category.categoryName, Orders.orderTime, Disc.songsAmount, Disc.duration, Disc.price from Users inner join Orders on Users.id = Orders.userId inner join Disc on Disc.discID = Orders.discID inner join Category on Disc.categoryID = Category.categoryID where Orders.isBought = 1 and Users.id = @userID", m_dbConnection))
                {
                    OrdersCommand.Parameters.Add(new SQLiteParameter("userID", ((User)Session["myUser"]).UserID));

                    using (SQLiteDataReader reader = OrdersCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Order()
                            {
                                OrderTime = DateTime.Parse(reader["orderTime"].ToString()),
                                Disc = new Disc()
                                {
                                    Name = reader["name"].ToString(),
                                    Artist = reader["artist"].ToString(),
                                    Category = new Category() { CategoryName = reader["categoryName"].ToString() },
                                    SongsAmount = int.Parse(reader["songsAmount"].ToString()),
                                    Duration = reader["duration"].ToString(),
                                    Price = float.Parse(reader["price"].ToString())
                                }
                            });
                        }
                    }
                }
            }

            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();
                using (SQLiteCommand SumCommand = new SQLiteCommand(@"Select Sum(Disc.price) as price from Disc inner join Orders on Disc.discID = Orders.discID where Orders.userid = @userID", m_dbConnection))
                {
                    SumCommand.Parameters.Add(new SQLiteParameter("userID", ((User)Session["myUser"]).UserID));

                    using (SQLiteDataReader reader = SumCommand.ExecuteReader())
                    {

                        while (reader.Read())
                        {

                            var totalPrice = float.Parse(reader["price"].ToString());
                            ViewBag.TotalPrice = totalPrice;

                        }
                    }

                }
            }
            return View(list);
        }
    }
}