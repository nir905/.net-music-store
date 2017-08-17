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
    public class CartController : BaseController
    {
        // GET: Cart
        public ActionResult Index(int msgCode = -1)
        {
            CartVM vm = new CartVM();
            List<Order> list = new List<Order>();
            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();
                SQLiteCommand storeCdCommand =
                        new SQLiteCommand(
                            @"select Disc.discID,Disc.name,Disc.artist,Category.categoryName,Disc.pictureUrl,Disc.price,added,duration,songsamount,Orders.amount,Orders.orderTime,Orders.orderID from Disc 
                                INNER JOIN Category on Category.categoryid = Disc.categoryid
								INNER JOIN Orders on Orders.discID = Disc.discID
								where Orders.userId=@userid and Orders.isBought=0",
                            m_dbConnection);
                storeCdCommand.Parameters.Add(new SQLiteParameter("userid", ((User)Session["myUser"]).UserID));
                using (SQLiteDataReader reader = storeCdCommand.ExecuteReader())
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
                            OrderID = int.Parse(reader["orderid"].ToString())
                        });
                    }
                }
            }
            vm.Orders = list;
            switch (msgCode)
            {
                case 0:
                    ViewBag.msg = "Problem with cart's shipping";
                    break;
                case 1:
                    ViewBag.msg = "No items selected!";
                    break;
                case 2:
                    ViewBag.msg = "No insert information correctly!";
                    break;
            }
            return View(vm);
        }

        [HttpPost]
        public ActionResult ShipCart(CartVM cart)
        {
            try
            {
                List<string> ids = new List<string>();
                for (var i = 0; i < cart.Orders.Count(); i++)
                {
                    Order order = cart.Orders[i];
                    if (order.IsChecked)
                        ids.Add("@id" + i);
                }
                if(!ids.Any())
                    return RedirectToAction("Index", "Cart", new { msgCode = 1 });
                if (string.IsNullOrEmpty(cart.Address) || string.IsNullOrEmpty(cart.City) || string.IsNullOrEmpty(cart.Country))
                    return RedirectToAction("Index", "Cart", new { msgCode = 2 });
                string param = String.Join(", ", ids.ToArray());//@id1, @id2..
                var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
                using (var m_dbConnection = new SQLiteConnection(connectionString))
                {
                    m_dbConnection.Open();
                    SQLiteCommand storeCdCommand =
                            new SQLiteCommand(
                                @"update Orders set address = @address, city = @city, country = @country,isbought = 1 where orderid in (" + param + ")",
                                m_dbConnection);
                    storeCdCommand.Parameters.Add(new SQLiteParameter("address", cart.Address));
                    storeCdCommand.Parameters.Add(new SQLiteParameter("city", cart.City));
                    storeCdCommand.Parameters.Add(new SQLiteParameter("country", cart.Country));
                    for (var i = 0; i < cart.Orders.Count; i++)
                    {
                        Order order = cart.Orders[i];
                        if (order.IsChecked)
                            storeCdCommand.Parameters.Add(new SQLiteParameter("id" + i, order.OrderID));
                    }
                    storeCdCommand.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", "Cart", new { msgCode = 0 });
            }
            return RedirectToAction("Success", "Cart");
        }

        public ActionResult Success()
        {
            return View();
        }
    }
}