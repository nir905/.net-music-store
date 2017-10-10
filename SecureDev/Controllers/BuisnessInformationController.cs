using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vladi2.App_Start;
using Vladi2.Helpers;
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
            float totalPrice = 0;

            try
            {
                using (var m_dbConnection = new SQLiteConnection(connectionString))
                {
                    m_dbConnection.Open();
                    using (SQLiteCommand OrdersCommand = new SQLiteCommand(@"SELECT Disc.name, Disc.artist, Disc.price, Disc.duration, Disc.songsAmount, Category.categoryName, Orders.orderTime, Orders.amount
                                                                         FROM Disc INNER JOIN Category ON Disc.categoryID = Category.categoryID
                                                                         INNER JOIN Orders ON Orders.discID = Disc.discID
                                                                         WHERE Orders.isBought = 1 AND Orders.userId = @userID", m_dbConnection))
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
                                totalPrice += int.Parse(reader["amount"].ToString()) * list.Last().Disc.Price;
                            }
                        }
                    }
                }
            }
            catch(SQLiteException)
            {
                Logger.WriteToLog(Logger.SQLLiteMsg);
                throw;
            }
            catch (Exception exception)
            {
                Logger.WriteToLog(exception);
                throw;
            }

            ViewBag.TotalPrice = totalPrice;
            return View(list);
        }
    }
}