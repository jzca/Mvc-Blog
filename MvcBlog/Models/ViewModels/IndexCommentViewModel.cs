using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MvcBlog.Models.Domain
{
    public class IndexCommentViewModel
    {
        public string Body { get; set; }
        public string ReasonUpdated { get; set; }
        public string UserEmail { get; set; }
    }
}