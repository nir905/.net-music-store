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
using Vladi2.Helpers;
using Vladi2.Models;

namespace Vladi2.Controllers
{
    [AuthAttr]
    public class EditProfileController : BaseController
    {
        public ActionResult Index()
        {
            return View((User)Session["myUser"]);
        }

        [HttpPost]
        public ActionResult Index(User user, HttpPostedFileBase file)
        {
            string path = "";
            ErrorValidation error = UserValidation.IsValidUser(user, "", false);

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
            file.SaveAs(Path.Combine(Server.MapPath(@"~\ProfileImages\"),valid.Message));

            //delete prev picture
            string oldFileName = Server.MapPath(((User)Session["myUser"]).PictureUrl);
            if (!oldFileName.Contains("default-user.png"))
                if (System.IO.File.Exists(oldFileName))
                {
                    System.IO.File.Delete(oldFileName);
                }

            ((User)Session["myUser"]).PictureUrl = @"~/ProfileImages/" + valid.Message;
            ((User)Session["myUser"]).FirstName = user.FirstName;
            ((User)Session["myUser"]).LastName = user.LastName;
            ((User)Session["myUser"]).Email = user.Email;
            ((User)Session["myUser"]).Phone = user.Phone;


            var connectionString = string.Format("DataSource={0}", Server.MapPath(@"~\Sqlite\db.sqlite"));
            using (var m_dbConnection = new SQLiteConnection(connectionString))
            {
                m_dbConnection.Open();

                using (SQLiteCommand updateUser = new SQLiteCommand("update users set firstName = @firstname, lastName = @lastname, email = @email, phone = @phone, pictureUrl = @pictureurl where userName = @username", m_dbConnection))
                {
                    updateUser.Parameters.Add(new SQLiteParameter("username", ((User)Session["myUser"]).UserName));
                    updateUser.Parameters.Add(new SQLiteParameter("pictureurl", ((User)Session["myUser"]).PictureUrl));
                    updateUser.Parameters.Add(new SQLiteParameter("firstname", user.FirstName));
                    updateUser.Parameters.Add(new SQLiteParameter("lastname", user.LastName));
                    updateUser.Parameters.Add(new SQLiteParameter("email", user.Email));
                    updateUser.Parameters.Add(new SQLiteParameter("phone", user.Phone));
                    updateUser.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index", "Profile");
        }
    }
}