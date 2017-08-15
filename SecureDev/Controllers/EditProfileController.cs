using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Vladi2.App_Start;
using Vladi2.Models;

namespace Vladi2.Controllers
{
    [AuthAttr]
    public class EditProfileController : BaseController
    {
        public ActionResult Index()
        {
            ViewData["tempUser"] = Session["myUser"];
            return View();
        }

        [HttpPost]
        public ActionResult Index(User user)
        {
            ViewData["tempUser"] = user;
            
            Regex regex = new Regex(@"^[a-zA-Z]{1,6}[-]{0,1}[a-zA-Z]{1,6}$");
            Match match = regex.Match(user.FirstName);

            if (!match.Success)
            {
                ViewBag.firstNameErr = "First Name must be between 2 - 12 letters";
                return View();
            }
            else
            {
                ViewBag.firstNameErr = null;
            }

            regex = new Regex(@"^[a-zA-Z]{1,8}[-]{0,1}[a-zA-Z]{1,8}$");
            match = regex.Match(user.LastName);

            if (!match.Success)
            {
                ViewBag.lastNameErr = "Last Name must be between 2 - 16 letters";
                return View();
            }
            else
            {
                ViewBag.lastNameErr = null;
            }

            regex = new Regex(@"^(([a-zA-Z0-9._%-]{2,30})@([a-zA-Z.-]{3,12})\.([a-zA-Z]{2,5}))$");
            match = regex.Match(user.Email);

            if (!match.Success)
            {
                ViewBag.emailErr = "Not a valid email";
                return View();
            }
            else
            {
                ViewBag.emailErr = null;
            }

            regex = new Regex(@"^[0-9]{10,10}$");
            match = regex.Match(user.Phone);

            if (!match.Success)
            {
                ViewBag.phoneErr = "Not a valid phone number";
                return View();
            }
            else
            {
                ViewBag.phoneErr = null;
            }

            Session["myUser"] = user;


            return RedirectToAction("Index", "Profile");
        }
    }
}