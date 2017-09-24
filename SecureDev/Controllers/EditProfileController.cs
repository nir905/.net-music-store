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
            UserResult result = user.UpdateProfile(file);

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

            return RedirectToAction("Index", "Profile");
        }
    }
}