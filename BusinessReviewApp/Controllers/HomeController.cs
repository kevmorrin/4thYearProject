using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessReviewApp.Models;

namespace BusinessReviewApp.Controllers
{
    public class HomeController : Controller
    {
        private UsersContext db = new UsersContext();

        public ActionResult Index()
        {
            ViewBag.Message = "View details about Irish businesses.";

            //Get the businesses from the DB
            return View(db.Businesses.ToList());
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
