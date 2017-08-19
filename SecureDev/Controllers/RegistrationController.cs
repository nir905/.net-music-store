using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Microsoft.Security.Application;
using Vladi2.Helpers;
using Vladi2.Models;

namespace Vladi2.Controllers
{
    public class RegistrationController : BaseController
    {
        public ActionResult Index()
        {
            if (Session["myUser"] == null)
            {
                HttpContext.Response.Cookies.Add(new HttpCookie("ASP.NET_SessionId"){ Expires = DateTime.Now.AddDays(-1)});
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
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public ActionResult Index(User user, HttpPostedFileBase file)
        {

            int permittedSizeInBytes = 4000000;//4mb
            string path = "";
            string connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));

            ErrorValidation error = UserValidation.IsValidUser(user, connectionString, true);

            switch (error.ErrorCode)
            {
                case ErrorValidation.Errors.UserNameTaken:
                case ErrorValidation.Errors.NotValidUserName:
                    ViewBag.userNameErr = error.Message;
                    return View(user);
                case ErrorValidation.Errors.NotValidPassword:
                    ViewBag.passwordErr = error.Message;
                    return View(user);
                case ErrorValidation.Errors.NotValidFirstName:
                    ViewBag.firstNameErr = error.Message;
                    return View(user);
                case ErrorValidation.Errors.NotValidLastName:
                    ViewBag.lastNameErr = error.Message;
                    return View(user);
                case ErrorValidation.Errors.NotValidEmail:
                    ViewBag.emailErr = error.Message;
                    return View(user);
                case ErrorValidation.Errors.NotValidPhone:
                    ViewBag.phoneErr = error.Message;
                    return View(user);
            }

            UploadFileValidation valid = UserValidation.UploadFile(file);
            if (valid.ErrorCode == UploadFileValidation.Errors.Error)
            {
                ViewBag.imageErr = valid.Message;
                return View(user);
            }
            if (valid.Message != "")
            {
                file.SaveAs(Path.Combine(Server.MapPath(@"~\ProfileImages\"), valid.Message));
                user.PictureUrl = @"~/ProfileImages/" + valid.Message;
            }
            else
            {
                user.PictureUrl = @"~/ProfileImages/default-user.png";
            }
            user.Password = UserValidation.sha256(user.Password);

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

    }
}