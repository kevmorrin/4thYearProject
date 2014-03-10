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

                    //Update Rating for business
                    calculateRating(review);
                    
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
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Review review)
        {
            if (ModelState.IsValid)
            {
                db.Entry(review).State = EntityState.Modified;
                db.SaveChanges();

                //Update Rating for business
                calculateRating(review);
                
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
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Review review = db.Reviews.Find(id);
            db.Reviews.Remove(review);
            db.SaveChanges();

            //Update Rating for business
            calculateRating(review);

            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        //Used for calculation of the rating of a business based on reviews
        public void calculateRating(Review review)
        {
            int totalNumberOfReviews = 0;
            int totalRating = 0;
            List<Review> reviews = db.Reviews.ToList();

            //Recalculate the combined review ratings
            foreach (var item in reviews)
            {
                //If item is the same as the business currently being reviewed
                if (item.BusinessID == review.BusinessID)
                {
                    totalNumberOfReviews++;
                    totalRating += item.Rating;
                }
            }

            Business b1 = new Business();
            b1 = db.Businesses.Find(review.BusinessID);
            
            //Check if there are no reviews to ensure there is no divide by 0 exception
            if (totalNumberOfReviews == 0)
            {
                b1.CombinedReviewRating = 0;//Set to 0 if no reviews
            }
            else
            {
                b1.CombinedReviewRating = totalRating / totalNumberOfReviews;//Calculate average rating
            }

            db.Entry(b1).State = EntityState.Modified;
            db.SaveChanges();
        }
    }
}