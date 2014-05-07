using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using BusinessReviewApp.Models;

namespace BusinessReviewApp.Controllers
{
    public class BusinessAPIController : ApiController
    {
        private UsersContext db = new UsersContext();

        // GET api/BusinessAPI/
        public IEnumerable<Business> GetAllBusinesses()
        {
            List<Business> businesses = new List<Business>();

            foreach (var item in db.Businesses)
            {
                businesses.Add(item);
            }

            return businesses;                                                     // 200 OK, weather serialized in response body
        }

        // GET api/BusinessAPI/value?county=Dublin&SearchString=Penneys
        public IEnumerable<Business> GetQueriedBusinesses(string value, string county, string searchString)
        {
            
            //Get the businesses from the DB
            List<Business> businesses = db.Businesses.ToList();

            if (!String.IsNullOrEmpty(searchString))
            {
                businesses = businesses.Where(s => s.Name.Contains(searchString)).ToList();
            }

            if (!String.IsNullOrEmpty(county))
            {
                businesses = businesses.Where(s => s.County.Contains(county)).ToList();
            }

            if (!string.IsNullOrEmpty(value))
            {
                businesses = businesses.Where(x => x.Category == value).ToList();
            }

            return businesses;
            // 200 OK, weather serialized in response body
        }
    }
}