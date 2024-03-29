﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcBlog.Models.ViewModels
{
    public class CreateEditPostViewModel
    {
        [Required]
        public string Title { get; set; }
        [Required]
        [AllowHtml]
        public string Body { get; set; }
        public HttpPostedFileBase Media { get; set; }
        public string MediaUrl { get; set; }
        public bool Published { get; set; }
    }
}