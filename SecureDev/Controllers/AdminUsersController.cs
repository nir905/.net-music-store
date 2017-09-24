using Microsoft.Security.Application;
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
    [AuthAttr(OnlyAdmin = true)]
    public class AdminUsersController : BaseController
    {
        public ActionResult Index()
        {
            List<User> usersList = new List<User>();

            try
            {
                var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
                using (var m_dbConnection = new SQLiteConnection(connectionString))
                {
                    m_dbConnection.Open();
                    SQLiteCommand usersCommand = new SQLiteCommand(@"SELECT userName, firstName, lastName, email, phone, pictureUrl, isAdmin 
                                                                 FROM Users
                                                                 WHERE userName != @user
                                                                 ORDER BY userName COLLATE NOCASE ASC", m_dbConnection);

                    usersCommand.Parameters.Add(new SQLiteParameter("user", (Session["myUser"] as User).UserName));
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

            return View(usersList);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult changeUsersPrivileges(List<User> usersList)
        {
            Models.User.changeUsersPrivileges(usersList);

            return RedirectToAction("Index", "Admin");
        }
    }
}