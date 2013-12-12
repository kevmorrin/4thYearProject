using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BusinessReviewApp.Models;

namespace BusinessReviewApp.Controllers
{
    public class ReviewController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /Review/

        public ActionResult Index()
        {
            var reviews = db.Reviews.Include(r => r.Business).Include(r => r.UserProfile);
            return View(reviews.ToList());
        }

        //
        // GET: /Review/Details/5

        public ActionResult Details(int id = 0)
        {
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            return View(review);
        }

        //
        // GET: /Review/Create
        [Authorize]
        public ActionResult Create()
        {
            ViewBag.BusinessID = new SelectList(db.Businesses, "BusinessID", "Name");
            ViewBag.UserId = new SelectList(db.UserProfiles, "UserId", "UserName");
            return View();
        }

        //
        // POST: /Review/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Review review)
        {
            if (ModelState.IsValid)
            {
                //Assume business review for that user already exists
                bool reviewAlreadyExists = false;

                //Check to see if there is already a review for the business for the user.
                List<Review> reviews = new List<Review>();

                //Foreach Review check if there is reviews for the business
                foreach (var item in db.Reviews)
                {
                    if(item.BusinessID == review.BusinessID)
                    {
                        reviews.Add(item);
                    }    
                }

                //Foreach review, check if the user has already added one.
                foreach (var item in reviews)
                {
                    if (item.UserId == review.UserId)
                    {
                        reviewAlreadyExists = true;
                    }
                }

                //Check if a review for this business from this user does not already exist
                if(reviewAlreadyExists == true)
                {
                    ModelState.AddModelError("", "You have already review this business and may not add another review.");
                }
                else
                {
                    //Add details
                    db.Reviews.Add(review);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
               
            }

            ViewBag.BusinessID = new SelectList(db.Businesses, "BusinessID", "Name", review.BusinessID);
            ViewBag.UserId = new SelectList(db.UserProfiles, "UserId", "UserName", review.UserId);
            return View(review);
        }

        //
        // GET: /Review/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            ViewBag.BusinessID = new SelectList(db.Businesses, "BusinessID", "Name", review.BusinessID);
            ViewBag.UserId = new SelectList(db.UserProfiles, "UserId", "UserName", review.UserId);
            return View(review);
        }

        //
        // POST: /Review/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Review review)
        {
            if (ModelState.IsValid)
            {
                db.Entry(review).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.BusinessID = new SelectList(db.Businesses, "BusinessID", "Name", review.BusinessID);
            ViewBag.UserId = new SelectList(db.UserProfiles, "UserId", "UserName", review.UserId);
            return View(review);
        }

        //
        // GET: /Review/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            return View(review);
        }

        //
        // POST: /Review/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Review review = db.Reviews.Find(id);
            db.Reviews.Remove(review);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}