using Microsoft.AspNet.Identity;
using MvcBlog;
using MvcBlog.Models;
using MvcBlog.Models.Domain;
using MvcBlog.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace PostDatabase.Controllers
{
    public class PostController : Controller
    {
        private ApplicationDbContext DbContext;

        private string fileExtensionForSavingPost;

        private Post postForSavingPost;

        public PostController()
        {
            DbContext = new ApplicationDbContext();
        }

        public ActionResult Index()
        {
            var appUserId = User.Identity.GetUserId();

            var model = DbContext.Posts
                .Where(p => p.UserId == appUserId)
                .Select(p => new IndexPostViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Body=p.Body
                }).ToList();

            return View(model);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(CreateEditPostViewModel formData)
        {
            return SavePost(null, formData);
        }

        private ActionResult SavePost(int? id, CreateEditPostViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var appUserId = User.Identity.GetUserId();

            if (DbContext.Posts.Any(p => p.UserId == appUserId &&
            p.Title == formData.Title &&
            (!id.HasValue || p.Id != id.Value)))
            {
                ModelState.AddModelError(nameof(CreateEditPostViewModel.Title),
                    "Post title should be unique");

                return View();
            }



            //Validating file upload
            if (formData.Media != null)
            {
                fileExtensionForSavingPost = Path.GetExtension(formData.Media.FileName).ToLower();

                if (!ImgHandler.AllowedFileExtensions.Contains(fileExtensionForSavingPost))
                {
                    ModelState.AddModelError("", "File extension is not allowed.");
                    return View();
                }
            }


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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(PostController.Index));
            }

            var appUserId = User.Identity.GetUserId();

            var post = DbContext.Posts.FirstOrDefault(
                p => p.Id == id.Value && p.UserId == appUserId);

            if (post == null)
            {
                return RedirectToAction(nameof(PostController.Index));
            }

            var model = new CreateEditPostViewModel();

            model.Title = post.Title;
            model.Body = post.Body;
            model.MediaUrl = post.MediaUrl;

            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(int id, CreateEditPostViewModel formData)
        {
            return SavePost(id, formData);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction(nameof(PostController.Index));
            }

            var appUserId = User.Identity.GetUserId();

            var Post = DbContext.Posts.FirstOrDefault(p => p.Id == id && p.UserId == appUserId);

            if (Post != null)
            {
                DbContext.Posts.Remove(Post);
                DbContext.SaveChanges();
            }

            return RedirectToAction(nameof(PostController.Index));
        }

        [HttpGet]
        public ActionResult Detail(int? id)
        {
            if (!id.HasValue)
                return RedirectToAction(nameof(PostController.Index));

            var appUserId = User.Identity.GetUserId();

            var post = DbContext.Posts.FirstOrDefault(p =>
            p.Id == id.Value);
            //&&p.UserId == appUserId);

            if (post == null)
                return RedirectToAction(nameof(PostController.Index));

            var model = new DetailPostViewModel();
            model.Title = post.Title;
            model.Body = post.Body;
            model.MediaUrl = post.MediaUrl;

            return View(model);
        }

    }
}