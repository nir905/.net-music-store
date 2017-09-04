using Microsoft.Security.Application;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Vladi2.Models;

namespace Vladi2.Controllers
{
    public class RegistrationController : BaseController
    {
        public ActionResult Index()
        {
            User user = new User()
            {
                UserName = "",
                Password = "",
                FirstName = "",
                Email = "",
                Phone = "",
                LastName = ""
            };
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(User user, HttpPostedFileBase file)
        {
            user.FirstName = Sanitizer.GetSafeHtmlFragment(user.FirstName);
            user.LastName = Sanitizer.GetSafeHtmlFragment(user.LastName);
            user.Email = Sanitizer.GetSafeHtmlFragment(user.Email);
            user.Phone = Sanitizer.GetSafeHtmlFragment(user.Phone);
            user.PictureUrl = Sanitizer.GetSafeHtmlFragment(user.PictureUrl);
            user.UserName = Sanitizer.GetSafeHtmlFragment(user.UserName);
            user.Password = Sanitizer.GetSafeHtmlFragment(user.Password);
        
            int permittedSizeInBytes = 4000000;//4mb
            string path = "";
            string connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));

            Regex regex;
            Match match;

            if (user.UserName != null)
            {
                regex = new Regex(@"^[a-zA-Z]{3,6}[_]{0,1}[a-zA-Z0-9]{0,6}$");
                match = regex.Match(user.UserName);

                if (!match.Success)
                {
                    ViewBag.userNameErr = "User Name must start with letters and be between 3 - 12 letters";
                    return View(user);
                }
                else
                {
                    ViewBag.userNameErr = null;

                    using (var m_dbConnection = new SQLiteConnection(connectionString))
                    {
                        m_dbConnection.Open();
                        using (SQLiteCommand checkUserNameCommand = new SQLiteCommand("SELECT username FROM Users Where userName = @username", m_dbConnection))
                        {
                            checkUserNameCommand.Parameters.Add(new SQLiteParameter("username", user.UserName));
                            using (SQLiteDataReader reader = checkUserNameCommand.ExecuteReader())
                            {
                                //userName has been already taken
                                if (reader.HasRows)
                                {
                                    ViewBag.userNameErr = "userName has been already taken";
                                }
                            }
                        }
                    }
                    if (ViewBag.userNameErr != null)
                        return View(user);
                }
            }
            else
            {
                ViewBag.userNameErr = "User Name must start with letters and be between 3 - 12 letters";
                return View(user);
            }

            if (user.Password != null)
            {
                //check at least 8 chars, at least one uppercase letter, lowercase, number
                regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d]{8,15}$");
                match = regex.Match(user.Password);

                if (!match.Success)
                {
                    ViewBag.passwordErr = "The password must be between 8 - 15 chars, at least 1 uppercase letter, 1 lowercase letter and 1 digit";
                    return View(user);
                }
                else
                {
                    ViewBag.passwordErr = null;
                }
            }
            else
            {
                ViewBag.passwordErr = "The password must be between 8 - 15 chars, at least 1 uppercase letter, 1 lowercase letter and 1 digit";
                return View(user);
            }


            if (user.FirstName != null)
            {
                regex = new Regex(@"^[a-zA-Z]{1,6}[-]{0,1}[a-zA-Z]{1,6}$");
                match = regex.Match(user.FirstName);

                if (!match.Success)
                {
                    ViewBag.firstNameErr = "First Name must be between 2 - 12 letters";
                    return View(user);
                }
                else
                {
                    ViewBag.firstNameErr = null;
                }
            }
            else
            {
                ViewBag.firstNameErr = "First Name must be between 2 - 12 letters";
                return View(user);
            }

            if (user.LastName != null)
            {
                regex = new Regex(@"^[a-zA-Z]{1,8}[-]{0,1}[a-zA-Z]{1,8}$");
                match = regex.Match(user.LastName);

                if (!match.Success)
                {
                    ViewBag.lastNameErr = "Last Name must be between 2 - 16 letters";
                    return View(user);
                }
                else
                {
                    ViewBag.lastNameErr = null;
                }
            }
            else
            {
                ViewBag.lastNameErr = "Last Name must be between 2 - 16 letters";
                return View(user);
            }

            if (user.Email != null)
            {
                regex = new Regex(@"^(([a-zA-Z0-9._%-]{2,30})@([a-zA-Z.-]{3,12})\.([a-zA-Z]{2,5}))$");
                match = regex.Match(user.Email);

                if (!match.Success)
                {
                    ViewBag.emailErr = "Not a valid email";
                    return View(user);
                }
                else
                {
                    ViewBag.emailErr = null;
                }
            }
            else
            {
                ViewBag.emailErr = "Not a valid email";
                return View(user);
            }

            if (user.Phone != null)
            {
                regex = new Regex(@"^[0-9]{10,10}$");
                match = regex.Match(user.Phone);

                if (!match.Success)
                {
                    ViewBag.phoneErr = "Not a valid phone number";
                    return View(user);
                }
                else
                {
                    ViewBag.phoneErr = null;
                }
            }
            else
            {
                ViewBag.phoneErr = "Not a valid phone number";
                return View(user);
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
                    return View(user);
                }

                if (file.ContentLength > permittedSizeInBytes)
                {
                    ViewBag.imageErr = "image cannot be more than 4MB";
                    return View(user);
                }
                else
                    ViewBag.imageErr = null;

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName).ToLower();

                path = Path.Combine(Server.MapPath(@"~\ProfileImages\"), fileName);
                file.SaveAs(path);


                user.PictureUrl = @"~/ProfileImages/" + fileName;
            }
            else
            {
                ViewBag.imageErr = null;
                user.PictureUrl = @"~/ProfileImages/default-user.png";
            }

            user.Password = sha256(user.Password);

            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();

                using (SQLiteCommand createUser = new SQLiteCommand("insert into users (userName, password, firstName, lastName, email, phone, pictureUrl, isAdmin, loginCounts, lastAttempt) values (@username, @password, @firstname, @lastname, @email, @phone, @pictureurl, 0, 0, datetime('now', 'localtime'))", m_dbConnection))
                {
                    createUser.Parameters.Add(new SQLiteParameter("username", user.UserName));
                    createUser.Parameters.Add(new SQLiteParameter("password", user.Password));
                    createUser.Parameters.Add(new SQLiteParameter("pictureurl", user.PictureUrl));
                    createUser.Parameters.Add(new SQLiteParameter("firstname", user.FirstName));
                    createUser.Parameters.Add(new SQLiteParameter("lastname", user.LastName));
                    createUser.Parameters.Add(new SQLiteParameter("email", user.Email));
                    createUser.Parameters.Add(new SQLiteParameter("phone", user.Phone));
                    createUser.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index", "Login");
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