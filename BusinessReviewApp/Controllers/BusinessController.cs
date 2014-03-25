using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using BusinessReviewApp.Models;
using Recaptcha.Web;
using Recaptcha.Web.Mvc;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Configuration;
using Microsoft.WindowsAzure;

namespace BusinessReviewApp.Controllers
{
    public class BusinessReviewViewModel
    {
        public Business businesses { get; set; }
        public List<Review> reviews {get; set; }
    }

//@model MusicStoreViewModel

    public class BusinessController : Controller
    {
        //BLOB Storage Variables
        //Retrieve storage account from connection string.
        //CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
        //If storage connecion string is equal to null use this
        static CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=lookitup;AccountKey=ysY8da07rhRIlj4FMYU9vc7kedNm5DClWw6eKPUw863Qxw46alF8hqxRZEfIBoFDUFxp/1w/4bhXmnuyA+6CZw==");

        // Create the blob client.
        static CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

        // Retrieve reference to a previously created container.
        static CloudBlobContainer container = blobClient.GetContainerReference("photos");

        private UsersContext db = new UsersContext();

        //
        // GET: /Business/

        public ActionResult Index()
        {
            return View(db.Businesses.ToList());
        }

        //
        // GET: /Business/Details/5

        public ActionResult Details(int id = 0)
        {
            BusinessReviewViewModel businessReviewVM = new BusinessReviewViewModel();
            businessReviewVM.businesses = db.Businesses.Find(id);
            if (businessReviewVM.businesses == null)
            {
                return HttpNotFound();
            }

            businessReviewVM.reviews = new List<Review>();
            foreach (var item in db.Reviews.ToList())
            {
                if (item.BusinessID == db.Businesses.Find(id).BusinessID)
                {
                    businessReviewVM.reviews.Add(item);
                }
            }
            return View(businessReviewVM);
        }

        //
        // GET: /Business/Create
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Business/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Business business)
        {
            RecaptchaVerificationHelper recaptchaHelper = this.GetRecaptchaVerificationHelper();

            //Check if Captcha is empty
            if (String.IsNullOrEmpty(recaptchaHelper.Response))
            {
                ModelState.AddModelError("", "Captcha answer cannot be empty.");
                return View(business);
            }

            RecaptchaVerificationResult recaptchaResult = await recaptchaHelper.VerifyRecaptchaResponseTaskAsync();
            
            //Check if captcha is not a success
            if (recaptchaResult != RecaptchaVerificationResult.Success)
            {
                //Return user to business page with an error
                ModelState.AddModelError("", "Incorrect captcha answer.");
                return View(business);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    //Add the current datetime value
                    business.DateTime = System.DateTime.Now;

                    //Add business if model and captcha are valid 
                    db.Businesses.Add(business);
                    
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    return View(business);
                }
            }
        }

        //
        // GET: /Business/Edit/5
        [Authorize]
        public ActionResult Edit(int id = 0)
        {
            Business business = db.Businesses.Find(id);
            if (business == null)
            {
                return HttpNotFound();
            }
            return View(business);
        }

        //
        // POST: /Business/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Business business)
        {
            if (ModelState.IsValid)
            {
                //Update datetime edited
                business.DateTime = System.DateTime.Now;
                //Update the Rating
                business.CombinedReviewRating = calculateRating(business);
                //Update the BLOBs
                UpdateBLOBs(business);

                db.Entry(business).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(business);
        }

        //
        // GET: /Business/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Business business = db.Businesses.Find(id);
            if (business == null)
            {
                //Delete all the BLOBs associated with the business
                DeleteBLOBs(business);

                return HttpNotFound();
            }
            return View(business);
        }

        //
        // POST: /Business/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Business business = db.Businesses.Find(id);
            db.Businesses.Remove(business);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }

        //Used for calculation of the rating of a business based on reviews
        public int calculateRating(Business business)
        {
            int totalNumberOfReviews = 0;
            int totalRating = 0;
            int averageRating = 0;
            List<Review> reviews = db.Reviews.ToList();

            //Recalculate the combined review ratings
            foreach (var item in reviews)
            {
                //If item is the same as the business currently being reviewed
                if (item.BusinessID == business.BusinessID)
                {
                    totalNumberOfReviews++;
                    totalRating += item.Rating;
                }
            }

            //Check if there are no reviews to ensure there is no divide by 0 exception
            if (totalNumberOfReviews == 0)
            {
                averageRating = 0;//Set to 0 if no reviews
            }
            else
            {
                averageRating = totalRating / totalNumberOfReviews;//Calculate average rating
            }

            return averageRating;
        }

        //Called when all blobs for a business need to be deleted
        public void DeleteBLOBs(Business business)
        {
            //Delete all the blobs associated with the account
            if (business.URLPhoto1 != null)
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(business.Name + business.Street + business.County + "Photo1.jpg");
                // Delete the blob.
                blockBlob.Delete();
            }
            if (business.URLPhoto2 != null)
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(business.Name + business.Street + business.County + "Photo2.jpg");
                // Delete the blob.
                blockBlob.Delete();
            }
            if (business.URLPhoto3 != null)
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(business.Name + business.Street + business.County + "Photo3.jpg");
                // Delete the blob.
                blockBlob.Delete();
            }
            if (business.URLPhoto4 != null)
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(business.Name + business.Street + business.County + "Photo4.jpg");
                // Delete the blob.
                blockBlob.Delete();
            }
            if (business.URLPhoto5 != null)
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(business.Name + business.Street + business.County + "Photo5.jpg");
                // Delete the blob.
                blockBlob.Delete();
            }
        }

        public void UpdateBLOBs(Business business)
        {
            /*-----------------------UPDATE BLOB STORAGE-----------------*/
            //Upload photos for the business if the local image URLs are entered
            if (business.URLPhoto1 != null)
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(business.Name + business.Street + business.County + "Photo1.jpg");
                using (var fileStream = System.IO.File.OpenRead(@business.URLPhoto1))
                {
                    //Change from upload to stream to another alternative e.g.
                    //blockBlob.UploadFromByteArray(parameters....);
                    blockBlob.UploadFromStream(fileStream);
                    //Set URL in database to point to BLOB
                    business.URLPhoto1 = "http://lookitup.blob.core.windows.net/photos/" + business.Name + business.Street + business.County + "Photo1.jpg";
                }
            }
            else
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(business.Name + business.Street + business.County + "Photo1.jpg");

                // Delete the blob.
                blockBlob.Delete();
            }

            if (business.URLPhoto2 != null)
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(business.Name + business.Street + business.County + "Photo2.jpg");
                using (var fileStream = System.IO.File.OpenRead(@business.URLPhoto2))
                {
                    blockBlob.UploadFromStream(fileStream);
                    business.URLPhoto2 = "http://lookitup.blob.core.windows.net/photos/" + business.Name + business.Street + business.County + "Photo2.jpg";

                }
            }

            if (business.URLPhoto3 != null)
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(business.Name + business.Street + business.County + "Photo3.jpg");
                using (var fileStream = System.IO.File.OpenRead(@business.URLPhoto3))
                {
                    blockBlob.UploadFromStream(fileStream);
                    business.URLPhoto3 = "http://lookitup.blob.core.windows.net/photos/" + business.Name + business.Street + business.County + "Photo3.jpg";
                }
            }

            if (business.URLPhoto4 != null)
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(business.Name + business.Street + business.County + "Photo4.jpg");
                using (var fileStream = System.IO.File.OpenRead(@business.URLPhoto4))
                {
                    blockBlob.UploadFromStream(fileStream);
                    business.URLPhoto4 = "http://lookitup.blob.core.windows.net/photos/" + business.Name + business.Street + business.County + "Photo4.jpg";
                }
            }

            if (business.URLPhoto5 != null)
            {
                CloudBlockBlob blockBlob = container.GetBlockBlobReference(business.Name + business.Street + business.County + "Photo5.jpg");
                using (var fileStream = System.IO.File.OpenRead(@business.URLPhoto5))
                {
                    blockBlob.UploadFromStream(fileStream);
                    business.URLPhoto5 = "http://lookitup.blob.core.windows.net/photos/" + business.Name + business.Street + business.County + "Photo5.jpg";
                }
            }
        }
    }
}