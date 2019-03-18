using Microsoft.AspNet.Identity;
using MvcBlog.Models;
using MvcBlog.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcBlog.Controllers
{
    public class CommentController : Controller
    {

        private ApplicationDbContext DbContext;

        private Comment commentForSaving;
        private Comment commentForDetail;

        public CommentController()
        {
            DbContext = new ApplicationDbContext();
        }


        // GET: Comment
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Create(CreateEditCommentViewModel formData)
        {
            return SaveComment(null, formData);
        }


        private ActionResult SaveComment(int? id, CreateEditCommentViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var appUserEm = User.Identity.GetUserName();
            var appUserId = User.Identity.GetUserId();

            if (DbContext.Comments.Any(p => p.UserEmail == appUserEm &&
            p.Body == formData.Body &&
            (!id.HasValue || p.Id != id.Value)))
            {
                ModelState.AddModelError(nameof(CreateEditCommentViewModel.Body),
                    "You have already commented this text.");

                return View();
            }



            if (!id.HasValue)
            {
                commentForSaving = new Comment();
                commentForSaving.DateCreated = DateTime.Now;
                commentForSaving.UserEmail = appUserEm;
                commentForSaving.UserId = appUserId;

                //Post.

                DbContext.Comments.Add(commentForSaving);
            }
            else
            {
                commentForSaving = DbContext.Comments.FirstOrDefault(
               p => p.Id == id && p.UserEmail == appUserEm);

                if (commentForSaving == null)
                {
                    return RedirectToAction(nameof(CommentController.Index));
                }
            }

            commentForSaving.Body = formData.Body;
            commentForSaving.ReasonUpdated = formData.ReasonUpdated;
            commentForSaving.DateUpdated = DateTime.Now;


            DbContext.SaveChanges();

            return RedirectToAction(nameof(CommentController.Index));
        }
    }
    
}