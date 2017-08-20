using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Vladi2.Controllers
{
    public class ErrorController : BaseController
    {
        public ActionResult Error404()
        {
            return View();
        }
    }
}