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

        // Currently disabled as there is no need for it
        // GET: /Review/

        /*public ActionResult Index()
        {
            var reviews = db.Reviews.Include(r => r.Business).Include(r => r.UserProfile);
            return View(reviews.ToList());
        }*/

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

        [Authorize]
        public ActionResult Create(int id)
        {
            ViewBag.businessID = id;
            return View(new Review());
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
                //Set the user equal to the currently logged in user
                foreach (var item in db.UserProfiles)
                {
                    if (item.UserName == User.Identity.Name)
                    {
                        review.UserId = item.UserId;
                    }
                }

                //Set the user equal to the business currently being reviewed
                foreach (var item in db.UserProfiles)
                {
                    if (item.UserName == User.Identity.Name)
                    {
                        review.UserId = item.UserId;
                    }
                }

                //Assume business review for that user doesnt already exists
                bool reviewAlreadyExists = false;

                //Check to see if there is already a review for the business for the user.
                List<Review> reviews = new List<Review>();

                //Foreach Review check if there is reviews for the business
                foreach (var item in db.Reviews)
                {
                    if (item.BusinessID == review.BusinessID)
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
                if (reviewAlreadyExists == true)
                {
                    ModelState.AddModelError("", "You have already reviewed this business and may not add another review.");
                }
                else
                {
                    //Add details
                    db.Reviews.Add(review);
                    db.SaveChanges();

                    //Update Rating for business
                    calculateRating(review);

                    return RedirectToAction("Index", "Business");
                }
            }
            ViewBag.businessID = review.BusinessID;
            return View(review);
        }

        //
        // GET: /Review/Edit/5
        [Authorize]
        public ActionResult Edit(int id)
        {
            Review review = db.Reviews.Find(id);
            if (review == null)
            {
                return HttpNotFound();
            }
            ViewBag.BusinessID = id;
            return View(review);
        }

        //
        // POST: /Review/Edit/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Review review)
        {
            bool isYourReview = false;
            if (ModelState.IsValid)
            {
                //Gets the currently logged in user
                foreach (var item in db.UserProfiles)
                {
                    if (item.UserName == User.Identity.Name)
                    {
                        //checks to see if they wrote the review
                        if (item.UserId == review.UserId)
                        {
                            isYourReview = true;
                        }

                    }
                }
                if (isYourReview == true)
                {
                    db.Entry(review).State = EntityState.Modified;
                    db.SaveChanges();

                    //Update Rating for business
                    calculateRating(review);

                    return RedirectToAction("Index", "Business");
                }

            }
            ModelState.AddModelError("", "You can only edit reviews that you wrote.");
            ViewBag.businessID = review.BusinessID;
            return View();
        }

        //
        // GET: /Review/Delete/5
        [Authorize]
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
            bool isYourReview = false;
            Review review = db.Reviews.Find(id);

            //Gets the currently logged in user
            foreach (var item in db.UserProfiles)
            {
                if (item.UserName == User.Identity.Name)
                {
                    //checks to see if they wrote the review
                    if (item.UserId == review.UserId)
                    {
                        isYourReview = true;
                    }
                 }
            }
            if (isYourReview == true)
            {
                db.Reviews.Remove(review);
                db.SaveChanges();

                //Update Rating for business
                calculateRating(review);
                return RedirectToAction("Index", "Business");
            }
            ModelState.AddModelError("", "You may only delete reviews you wrote.");
            return View(review);
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