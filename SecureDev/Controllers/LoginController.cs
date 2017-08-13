using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Vladi2.Models;

namespace Vladi2.Controllers
{
    public class LoginController : BaseController
    {
        public ActionResult Index()
        {
            HttpCookie SID = HttpContext.Request.Cookies["SID"];
            if (SID == null || Session["myUser"] == null || SID.Value != HttpContext.Session.SessionID)
                return View();
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public ActionResult Index(User user)
        {
            if(String.IsNullOrEmpty(user.UserName) || String.IsNullOrEmpty(user.Password))
            {
                ViewBag.ErrorMsg = "Please enter user name and password!";
                return View();
            }
            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();
                using (SQLiteCommand LoginCommand = new SQLiteCommand("SELECT id,username,password,firstname,lastname,isadmin,logincounts,lastattempt FROM Users Where userName = @username", m_dbConnection))
                {
                    LoginCommand.Parameters.Add(new SQLiteParameter("username", user.UserName));
                    using (SQLiteDataReader reader = LoginCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            User myUser = new User()
                            {
                                UserID = int.Parse(reader["id"].ToString()),
                                UserName = reader["username"].ToString(),
                                Password = reader["password"].ToString(),
                                FirstName = reader["firstname"].ToString(),
                                LastName = reader["lastname"].ToString(),
                                IsAdmin = reader["isadmin"].ToString() == "1",
                                CountsAttempts = int.Parse(reader["logincounts"].ToString()),
                                LastAttempt = DateTime.Parse(reader["lastattempt"].ToString())
                            };

                            if (myUser.CountsAttempts < 5 || (DateTime.Now - myUser.LastAttempt).TotalMinutes>=20)
                            {
                                if (sha256(user.Password) == myUser.Password)//SHA256
                                {                                 
                                    //clear unseccess attempts
                                    using (SQLiteCommand clearUnseccess = new SQLiteCommand("update users set logincounts = 0, lastattempt = datetime('now', 'localtime') where username = @username", m_dbConnection))
                                    {
                                        clearUnseccess.Parameters.Add(new SQLiteParameter("username", user.UserName));
                                        clearUnseccess.ExecuteNonQuery();
                                    }
                                    Session["myUser"] = myUser;
                                    HttpContext.Response.SetCookie(new HttpCookie("SID", HttpContext.Session.SessionID));
                                    return RedirectToAction("Index", "Home");
                                }
                                else
                                {
                                    //Update Unseccessful attempts
                                    using (SQLiteCommand AttemptsCommand = new SQLiteCommand("update users set logincounts = (select logincounts +1 from users where username = @username), lastattempt = datetime('now', 'localtime') where username = @username", m_dbConnection))
                                    {
                                        AttemptsCommand.Parameters.Add(new SQLiteParameter("username", user.UserName));
                                        AttemptsCommand.ExecuteNonQuery();
                                    }
                                }
                            }
                            else
                            {
                                ViewBag.ErrorMsg = "You got banned! Please wait 20 minutes";
                                return View();
                            }
                        }
                    }
                }
            }
            ViewBag.ErrorMsg = "User name or Password is incorrect";
            return View();
        }

        string sha256(string password)
        {
            System.Security.Cryptography.SHA256Managed crypt = new System.Security.Cryptography.SHA256Managed();
            string hash = "";
            byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (byte theByte in crypto)
                hash += theByte.ToString("X2");
            return hash;
        }
    }
}