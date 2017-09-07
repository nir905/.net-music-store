using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Vladi2.App_Start;
using Vladi2.Models;
using Microsoft.Security.Application;

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
        [ValidateAntiForgeryToken]
        public ActionResult Index(User user, HttpPostedFileBase file)
        {
            //TODO: user.UpdateProfile(file);
            //lion&maria - I Moved all the code from UserValidation to User. just need to call <user.UpdateProfile(file)> here and make switch-case for all errors

            user.FirstName = Sanitizer.GetSafeHtmlFragment(user.FirstName);
            user.LastName = Sanitizer.GetSafeHtmlFragment(user.LastName);
            user.Email = Sanitizer.GetSafeHtmlFragment(user.Email);
            user.Phone = Sanitizer.GetSafeHtmlFragment(user.Phone);
            user.PictureUrl = Sanitizer.GetSafeHtmlFragment(user.PictureUrl);
            

            int permittedSizeInBytes = 4000000;//4mb
            string path = "";

            Regex regex;
            Match match;

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


                ((User)Session["myUser"]).PictureUrl = @"~/ProfileImages/" + fileName;
            }
            else
                ViewBag.imageErr = null;

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