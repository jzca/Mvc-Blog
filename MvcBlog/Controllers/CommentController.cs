using Microsoft.AspNet.Identity;
using MvcBlog.Models;
using MvcBlog.Models.Domain;
using PostDatabase.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;

namespace MvcBlog.Controllers
{
    public class CommentController : Controller
    {

        private ApplicationDbContext DbContext;

        private Comment commentForSaving;

        public CommentController()
        {
            DbContext = new ApplicationDbContext();
        }


        // GET: Comment
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Moderator")]
        public ActionResult Delete(int? id, string slug)
        {

            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var appUserId = User.Identity.GetUserId();

            var commentToDel = DbContext.Comments.FirstOrDefault(p => p.Id == id);

            if (commentToDel != null)
            {
                DbContext.Comments.Remove(commentToDel);
                DbContext.SaveChanges();
            }

            return RedirectToAction(nameof(PostController.DetailBySlug), "Post", new { slug = slug });
        }

        [HttpGet]
        [Authorize(Roles = "Admin, Moderator")]
        public ActionResult Edit(int? id)
        {

            if (!id.HasValue)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var comment = DbContext.Comments.FirstOrDefault(
                p => p.Id == id.Value);

            if (comment == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.ExpectationFailed);
            }

            var model = new EditCommentViewModel();

            model.Body = comment.Body;
            model.ReasonUpdated = comment.ReasonUpdated;
            model.UserEmail = comment.UserEmail;
            model.DateCreated = comment.DateCreated;
            model.DateUpdated = comment.DateUpdated;

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Moderator")]
        public ActionResult Edit(int id, EditCommentViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            //var appUserEm = User.Identity.GetUserName();
            //var appUserId = User.Identity.GetUserId();

            if (DbContext.Comments.Any(p => p.Body == formData.Body && p.Id != id))
            {
                ModelState.AddModelError(nameof(EditCommentViewModel.Body),
                    "The comment you edit is the same one as it is!");

                return View();
            }


            commentForSaving = DbContext.Comments.FirstOrDefault(
                p => p.Id == id);
            var post = DbContext.Posts.FirstOrDefault(p => p.Id == commentForSaving.PostId);

            commentForSaving.DateUpdated = DateTime.Now;
            commentForSaving.Body = formData.Body;
            commentForSaving.ReasonUpdated = formData.ReasonUpdated;

            if (commentForSaving == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.ExpectationFailed);
                //return RedirectToAction(nameof(CommentController.Index));
            }

            DbContext.SaveChanges();

            return RedirectToAction(nameof(PostController.DetailBySlug), "Post", new { slug = post.Slug });
        }

    }
}