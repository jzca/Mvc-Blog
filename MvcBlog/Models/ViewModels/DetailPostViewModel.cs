﻿using MvcBlog.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace MvcBlog.Models.ViewModels
{
    public class DetailPostViewModel
    {
        //public int Id { get; set; }

        public string Title { get; set; }
        public string Body { get; set; }

        public string MediaUrl { get; set; }

        public string Slug { get; set; }

        public List<Comment> Comments { get; set; }
        //public bool Published { get; set; }
        //public DateTime DateCreated { get; set; }
        //public DateTime? DateUpdated { get; set; }

        public DetailPostViewModel()
        {
            Comments = new List<Comment>();
        }
    }
}