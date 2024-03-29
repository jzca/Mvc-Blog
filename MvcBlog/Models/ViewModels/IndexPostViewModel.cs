﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcBlog.Models.ViewModels
{
    public class IndexPostViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Body { get; set; }
        public bool Published { get; set; }
        public string Slug { get; set; }
        //public DateTime DateCreated { get; set; }
        //public DateTime? DateUpdated { get; set; }
    }
}