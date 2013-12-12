using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BusinessReviewApp.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public partial class Review
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int ReviewID { get; set; }
        public string Description { get; set; }
        public int Rating { get; set; }
        public int BusinessID { get; set; }
        public int UserId { get; set; }

        public virtual Business Business { get; set; }
        public virtual UserProfile UserProfile { get; set; }
    }
}
