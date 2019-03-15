﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MvcBlog.Models.Domain
{
    public class Comment
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public string ReasonUpdated { get; set; }

        public virtual ApplicationUser User { get; set; }

    }
}