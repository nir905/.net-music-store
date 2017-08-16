using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vladi2.Models;

namespace Vladi2.Controllers
{
    public class ForumController : Controller
    {
        // GET: Forum
        public ActionResult Index()
        {
            return View(new List<Topic>()
            {
                new Topic()
                {
                    Title = "bla",
                },
                new Topic()
                {
                    Title = "bla2"
                }
            });
        }

        public ActionResult ReadTopic(int id)
        {
            return View(new Topic()
            {
                Title = "bla"+ id
            });
        }
    }
}