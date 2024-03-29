﻿using MvcBlog.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcBlog.Models.ViewModels
{
    public class IndexHomeViewModel
    {
        public string Text { get; set; }

        public int Id { get; set; }

        public string Title { get; set; }
        public string Body { get; set; }

        public string MediaUrl { get; set; }

        public DateTime DateCreated { get; set; }

        public string Slug { get; set; }

        public int AmtComment { get; set; }

    }
}