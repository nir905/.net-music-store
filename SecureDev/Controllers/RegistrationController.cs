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
            UserResult result = user.Register(file);

            if (result.Status == UserResult.Statuses.NotValidUserName || result.Status == UserResult.Statuses.UserNameTaken)
            {
                ViewBag.userNameErr = result.Message;
                return View(user);
            }
            else
                ViewBag.userNameErr = null;

            if (result.Status == UserResult.Statuses.NotValidPassword)
            {
                ViewBag.passwordErr = result.Message;
                return View(user);
            }
            else
                ViewBag.passwordErr = null;

            if (result.Status == UserResult.Statuses.NotValidFirstName)
            {
                ViewBag.firstNameErr = result.Message;
                return View(user);
            }
            else
                ViewBag.firstNameErr = null;

            if (result.Status == UserResult.Statuses.NotValidLastName)
            {
                ViewBag.lastNameErr = result.Message;
                return View(user);
            }
            else
                ViewBag.lastNameErr = null;

            if (result.Status == UserResult.Statuses.NotValidEmail)
            {
                ViewBag.emailErr = result.Message;
                return View(user);
            }
            else
                ViewBag.emailErr = null;

            if (result.Status == UserResult.Statuses.NotValidPhone)
            {
                ViewBag.phoneErr = result.Message;
                return View(user);
            }
            else
                ViewBag.phoneErr = null;

            if (result.Status == UserResult.Statuses.NotValidFile)
            {
                ViewBag.imageErr = result.Message;
                return View(user);
            }
            else
                ViewBag.imageErr = null;

            return RedirectToAction("Index", "Login");
        }
    }
}