using MvcBlog.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace MvcBlog.Models.ViewModels
{
    public class DetailPostViewModel
    {
        public string Title { get; set; }
        public string Body { get; set; }

        //public string CommentBody { get{
        //        CommentBody=
        //    }; }

        public string MediaUrl { get; set; }

        public string Slug { get; set; }

        public List<Comment> Comments { get; set; }

        public DetailPostViewModel()
        {
            Comments = new List<Comment>();
        }
    }
}