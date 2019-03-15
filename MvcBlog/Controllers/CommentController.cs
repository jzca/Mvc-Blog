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

        private string fileExtensionForSavingPost;

        private Post postForSavingPost;
        private Post postForDetail;


        // GET: Comment
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public ActionResult Create(CreateEditCommentViewModel formData)
        {
            return SavePost(null, formData);
        }


        private ActionResult SavePost(int? id, CreateEditCommentViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var appUserId = User.Identity.GetUserId();

            //if (DbContext.Posts.Any(p => p.UserId == appUserId &&
            //(!id.HasValue || p.Id != id.Value)))
            //{
            //    ModelState.AddModelError(nameof(CreateEditCommentViewModel.Title),
            //        "Post title should be unique");

            //    return View();
            //}



            if (!id.HasValue)
            {
                postForSavingPost = new Post();
                postForSavingPost.UserId = appUserId;
                postForSavingPost.DateCreated = DateTime.Now;


                DbContext.Posts.Add(postForSavingPost);
            }
            else
            {
                postForSavingPost = DbContext.Posts.FirstOrDefault(
               p => p.Id == id && p.UserId == appUserId);

                if (postForSavingPost == null)
                {
                    return RedirectToAction(nameof(PostController.Index));
                }
            }

            postForSavingPost.Title = formData.Title;
            postForSavingPost.Body = formData.Body;
            postForSavingPost.Published = formData.Published;
            postForSavingPost.DateUpdated = DateTime.Now;


            //Handling file upload
            if (formData.Media != null)
            {
                if (!Directory.Exists(ImgHandler.MappedUploadFolder))
                {
                    Directory.CreateDirectory(ImgHandler.MappedUploadFolder);
                }

                var fileName = formData.Media.FileName;
                var fullPathWithName = ImgHandler.MappedUploadFolder + fileName;

                formData.Media.SaveAs(fullPathWithName);

                postForSavingPost.MediaUrl = ImgHandler.ImgUploadFolder + fileName;
            }


            DbContext.SaveChanges();

            return RedirectToAction(nameof(PostController.Index));
        }
    }
    
}