using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vladi2.App_Start;

namespace Vladi2.Controllers
{
    public class ProfileController : BaseController
    {
        [AuthAttr]
        public ActionResult Index()
        {
            return View();

        }


    }
}