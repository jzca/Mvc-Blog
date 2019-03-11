using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcBlog.Models.ViewModels
{
    public class IndexHomeViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Body { get; set; }
        public string BodySliced { get => Body; set=>Body.Substring(0, 30);}

        public string MediaUrl { get; set; }

        //public bool Published { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateCreatedSliced { get => DateCreated; set => DateCreated.ToString("HH:mm dddd - MMMM dd yyyy"); }
        //public DateTime? DateUpdated { get; set; }

    }
}