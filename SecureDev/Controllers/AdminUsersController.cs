using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vladi2.Models;

namespace Vladi2.Controllers
{
    public class AdminUsersController : Controller
    {
        // GET: AdminUsers
        public ActionResult Index()
        {
            List<User> usersList = new List<User>();

            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();
                SQLiteCommand usersCommand = new SQLiteCommand(@"SELECT userName, firstName, lastName, email, phone, pictureUrl, isAdmin 
                                                                 FROM Users
                                                                 ORDER BY userName COLLATE NOCASE ASC", m_dbConnection);
                using (SQLiteDataReader reader = usersCommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        usersList.Add(new User()
                        {
                            UserName = reader["userName"].ToString(),
                            FirstName = reader["firstName"].ToString(),
                            Email = reader["email"].ToString(),
                            Phone = reader["phone"].ToString(),
                            LastName = reader["lastName"].ToString(),
                            IsAdmin = reader["isAdmin"].ToString() == "1",
                            PictureUrl = reader["pictureUrl"].ToString()
                        });
                    }
                }
            }



                return View(usersList);
        }
    }
}