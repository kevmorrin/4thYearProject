using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BusinessReviewApp.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "View details about Irish businesses.";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "LookItUp allows you to view Irish business details, amend their details and write reviews.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact information.";

            return View();
        }
    }
}
