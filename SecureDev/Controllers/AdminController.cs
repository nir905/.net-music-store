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
    public class AdminController : BaseController
    {
        // GET: Admin
        public ActionResult Index()
        {
            List<Disc> discsList = new List<Disc>();
            List<User> usersList = new List<User>();
            List<Category> catsList = new List<Category>();
            try
            {
                var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
                using (var m_dbConnection = new SQLiteConnection(connectionString))
                {
                    m_dbConnection.Open();
                    SQLiteCommand discCommand = new SQLiteCommand(@"select name, artist from Disc order by datetime(added, 'localtime') desc LIMIT 5", m_dbConnection);
                    using (SQLiteDataReader reader = discCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            discsList.Add(new Disc()
                            {
                                Name = reader["name"].ToString(),
                                Artist = reader["artist"].ToString(),

                            });
                        }
                    }

                    SQLiteCommand usersCommand = new SQLiteCommand(@"select firstname, lastname from Users order by id desc LIMIT 5", m_dbConnection);
                    using (SQLiteDataReader reader = usersCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usersList.Add(new User()
                            {
                                FirstName = reader["firstname"].ToString(),
                                LastName = reader["lastname"].ToString(),

                            });
                        }
                    }

                    SQLiteCommand catsCommand = new SQLiteCommand(@"select categoryName from category order by categoryid desc LIMIT 5", m_dbConnection);
                    using (SQLiteDataReader reader = catsCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            catsList.Add(new Category()
                            {
                                CategoryName = reader["categoryName"].ToString(),

                            });
                        }
                    }
                }
                return View(new AdminVM()
                {
                    Discs = discsList,
                    Users = usersList,
                    Categories = catsList
                });
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