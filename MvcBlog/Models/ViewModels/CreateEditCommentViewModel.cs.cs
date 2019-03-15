using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MvcBlog.Models.Domain
{
    public class CreateEditCommentViewModel
    {
        [Required]
        public string Body { get; set; }
        [Required]
        public string ReasonUpdated { get; set; }


    }
}