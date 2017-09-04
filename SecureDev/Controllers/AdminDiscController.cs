using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Microsoft.Security.Application;
using Vladi2.App_Start;
using Vladi2.Models;

namespace Vladi2.Controllers
{
    [AuthAttr(OnlyAdmin = true)]
    public class AdminDiscController : BaseController
    {
        private List<Category> GetCategories()
        {
            List<Category> categories = new List<Category>();
            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();
                SQLiteCommand categoriesCdCommand = new SQLiteCommand(
                    @"select categoryName,categoryid from category",
                    m_dbConnection);
                using (SQLiteDataReader reader = categoriesCdCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        categories.Add(new Category()
                        {
                            CategoryID = int.Parse(reader["categoryid"].ToString()),
                            CategoryName = reader["categoryName"].ToString()
                        });
                    }
                }
            }
            return categories;
        }

        // GET: AdminDisc
        public ActionResult Index()
        {
            List<Disc> list = new List<Disc>();
            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();
                SQLiteCommand storeCdCommand = new SQLiteCommand(
                    @"select Disc.discID,Disc.name,Disc.artist,Category.categoryName,added,Disc.pictureUrl,Disc.price,duration,songsamount from Disc 
                                INNER JOIN Category on Category.categoryid = Disc.categoryid order by datetime(added, 'localtime') desc",
                    m_dbConnection);
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
                            Price = float.Parse(reader["price"].ToString()),
                            DiscAdded = DateTime.Parse(reader["added"].ToString()),
                            Duration = reader["duration"].ToString(),
                            SongsAmount = int.Parse(reader["songsamount"].ToString())
                        });
                    }
                }
            }
            return View(list);
        }

        public ActionResult Create()
        {
            ViewBag.Categories = GetCategories();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Disc disc, HttpPostedFileBase file)
        {
            bool isValid = ValidForm(disc, file);
            if(!isValid)
                return View(disc);
            if (file != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName).ToLower();
                string path = Path.Combine(Server.MapPath(@"~\CD_Images\"), fileName);
                file.SaveAs(path);
                disc.PictureUrl = fileName;
            }
            else
            {
                disc.PictureUrl = "no-image.png";
            }
            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();
                SQLiteCommand createDiscCommand = new SQLiteCommand(
                    @"insert into disc (name,artist,categoryid,pictureurl,price,duration,songsamount,added)
                                        values (@name,@artist,@categoryid,@pictureurl,@price,@duration,@songsamount,@added)",
                    m_dbConnection);
                createDiscCommand.Parameters.Add(new SQLiteParameter("name", disc.Name));
                createDiscCommand.Parameters.Add(new SQLiteParameter("artist", disc.Artist));
                createDiscCommand.Parameters.Add(new SQLiteParameter("categoryid", disc.Category.CategoryID));
                createDiscCommand.Parameters.Add(new SQLiteParameter("pictureurl", disc.PictureUrl));
                createDiscCommand.Parameters.Add(new SQLiteParameter("price", disc.Price));
                createDiscCommand.Parameters.Add(new SQLiteParameter("duration", disc.Duration));
                createDiscCommand.Parameters.Add(new SQLiteParameter("songsamount", disc.SongsAmount));
                createDiscCommand.Parameters.Add(new SQLiteParameter("added", DateTime.Now));
                createDiscCommand.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int id)
        {
            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();

                SQLiteCommand discCdCommand = new SQLiteCommand(@"select pictureUrl from Disc where discid=@discid",
                    m_dbConnection);
                discCdCommand.Parameters.Add(new SQLiteParameter("discid", id));
                string image = discCdCommand.ExecuteScalar().ToString();
                string path = Path.Combine(Server.MapPath(@"~\CD_Images\"), image);
                if (System.IO.File.Exists(path) && image != "no-image.png")
                    System.IO.File.Delete(path);

                SQLiteCommand deleteDiscCommand = new SQLiteCommand(@"delete from disc where discid=@discid",
                    m_dbConnection);
                deleteDiscCommand.Parameters.Add(new SQLiteParameter("discid", id));
                deleteDiscCommand.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            ViewBag.Categories = GetCategories();
            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();
                SQLiteCommand discCdCommand =
                    new SQLiteCommand(
                        @"select Disc.discID,Disc.name,Disc.artist,Category.categoryid,Disc.pictureUrl,Disc.price,added,duration,songsamount from Disc 
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
                            Category = new Category() {CategoryID = Convert.ToInt32(reader["categoryid"].ToString())},
                            PictureUrl = reader["pictureurl"].ToString(),
                            Price = float.Parse(reader["price"].ToString()),
                            DiscAdded = Convert.ToDateTime(reader["added"].ToString()),
                            Duration = reader["duration"].ToString(),
                            SongsAmount = int.Parse(reader["songsamount"].ToString())
                        });
                    }
                }
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Disc disc, HttpPostedFileBase file)
        {
            bool isValid = ValidForm(disc, file);
            if (!isValid)
                return View(disc);
            if (file != null)
            {
                string path = Path.Combine(Server.MapPath(@"~\CD_Images\"), disc.PictureUrl);
                if (System.IO.File.Exists(path) && disc.PictureUrl != "no-image.png")
                    System.IO.File.Delete(path);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName).ToLower();
                string newpath = Path.Combine(Server.MapPath(@"~\CD_Images\"), fileName);
                file.SaveAs(newpath);
                disc.PictureUrl = fileName;
            }
            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();
                SQLiteCommand createDiscCommand = new SQLiteCommand(@"update disc  set name=@name,artist=@artist,categoryid=@categoryid,pictureurl=@pictureurl,price=@price,duration=@duration,songsamount=@songsamount where discid=@discid",m_dbConnection);
                createDiscCommand.Parameters.Add(new SQLiteParameter("name", disc.Name));
                createDiscCommand.Parameters.Add(new SQLiteParameter("artist", disc.Artist));
                createDiscCommand.Parameters.Add(new SQLiteParameter("categoryid", disc.Category.CategoryID));
                createDiscCommand.Parameters.Add(new SQLiteParameter("pictureurl", disc.PictureUrl));
                createDiscCommand.Parameters.Add(new SQLiteParameter("price", disc.Price));
                createDiscCommand.Parameters.Add(new SQLiteParameter("duration", disc.Duration));
                createDiscCommand.Parameters.Add(new SQLiteParameter("songsamount", disc.SongsAmount));
                createDiscCommand.Parameters.Add(new SQLiteParameter("discid", disc.DiscID));
                createDiscCommand.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        public bool ValidForm(Disc disc, HttpPostedFileBase file)
        {
            disc.Name = disc.Name == null ? "" : Sanitizer.GetSafeHtmlFragment(disc.Name);
            disc.Artist = disc.Artist == null ? "" : Sanitizer.GetSafeHtmlFragment(disc.Artist);
            disc.Duration = disc.Duration == null ? "" : Sanitizer.GetSafeHtmlFragment(disc.Duration);
            if (disc.Name.Length < 10 || disc.Name.Length > 30)
            {
                ViewBag.nameErr = "Disc name must be between 10 to 30 characters";
                ViewBag.Categories = GetCategories();
                return false;
            }
            if (disc.Artist.Length < 10 || disc.Artist.Length > 30)
            {
                ViewBag.artistErr = "Artist name must be between 10 to 30 characters";
                ViewBag.Categories = GetCategories();
                return false;
            }
            if (disc.Price <= 0 || disc.Price >= 99999)
            {
                ViewBag.priceErr = "Price must be a number between 0  and 99999";
                ViewBag.Categories = GetCategories();
                return false;
            }
            Regex regex = new Regex(@"^(?:[0-9][0-9]):[0-5][0-9]$");
            if (!regex.Match(disc.Duration).Success)
            {
                ViewBag.durationErr = "Duration must be in XX:XX format";
                ViewBag.Categories = GetCategories();
                return false;
            }
            if (disc.SongsAmount <= 0)
            {
                ViewBag.amountErr = "Songs amount must be grater then 0";
                ViewBag.Categories = GetCategories();
                return false;
            }
            if (file != null)
            {
                if (file.ContentType.ToLower() != "image/jpg" &&
                    file.ContentType.ToLower() != "image/jpeg" &&
                    file.ContentType.ToLower() != "image/pjpeg" &&
                    file.ContentType.ToLower() != "image/gif" &&
                    file.ContentType.ToLower() != "image/x-png" &&
                    file.ContentType.ToLower() != "image/png" &&
                    Path.GetExtension(file.FileName).ToLower() != ".jpg" &&
                    Path.GetExtension(file.FileName).ToLower() != ".png" &&
                    Path.GetExtension(file.FileName).ToLower() != ".gif" &&
                    Path.GetExtension(file.FileName).ToLower() != ".jpeg")
                {
                    ViewBag.imageErr = "Only images are allowed";
                    ViewBag.Categories = GetCategories();
                    return false;
                }
            }
            return true;
        }
    }
}