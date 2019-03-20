using Microsoft.AspNet.Identity;
using MvcBlog;
using MvcBlog.Controllers;
using MvcBlog.Models;
using MvcBlog.Models.Domain;
using MvcBlog.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Security;

namespace PostDatabase.Controllers
{
    public class PostController : Controller
    {
        private ApplicationDbContext DbContext;

        private string fileExtensionForSavingPost;

        private Post postForSavingPost;
        private Comment commentForSaving;
        private Post postForDetail;

        public PostController()
        {
            DbContext = new ApplicationDbContext();
        }

        public ActionResult Index()
        {
            var appUserId = User.Identity.GetUserId();

            var model = DbContext.Posts
                .OrderBy(p => p.Published)
                .Select(p => new IndexPostViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Body = p.Body,
                    Published = p.Published,
                    Slug = p.Slug
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
        [Authorize(Roles = "Admin")]
        public ActionResult Create(CreateEditPostViewModel formData)
        {
            return SavePost(null, formData);
        }

        private static string RemoveSpecialCharacters(string str)
        {
            // input;replace by matching any pattern of a-z and nums;repalce with space if doesnt match my pattern;
            return Regex.Replace(str, "[^a-zA-Z0-9_.]+", " ", RegexOptions.Compiled);
            // medium.com/factory-mind/regex-tutorial-a-simple-cheatsheet-by-examples-649dc1c3f285
            // docs.microsoft.com/en-us/dotnet/api/system.text.regularexpressions.regex.replace?view=netframework-4.7.2
        }

        private ActionResult SavePost(int? id, CreateEditPostViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var appUserId = User.Identity.GetUserId();

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

                // Make the Slug
                //RemoveSpecialCharacters
                var titleRmSpCharEd = RemoveSpecialCharacters(formData.Title).Trim();
                //Split by space,then Join with dash
                var formatedSlug = string.Join("-", titleRmSpCharEd.Split(' '));
                var lowerFormatedSlug = formatedSlug.ToLower();
                //Check if name repeated
                var repeatedSlug = DbContext.Posts.FirstOrDefault(
                    p => p.Slug == lowerFormatedSlug);
                if (repeatedSlug != null)
                {
                    // Create a 4 digit code
                    var shortCode = GetHashCode().ToString().Substring(0, 4);
                    // Concat it to the slug
                    lowerFormatedSlug = $"{lowerFormatedSlug}-{shortCode}";
                }
                // End of Make the Slug

                //Saving it
                postForSavingPost.Slug = lowerFormatedSlug;


                DbContext.Posts.Add(postForSavingPost);
            }
            else
            {
                postForSavingPost = DbContext.Posts.FirstOrDefault(
               p => p.Id == id && p.UserId == appUserId);

                postForSavingPost.DateUpdated = DateTime.Now;

                if (postForSavingPost == null)
                {
                    return RedirectToAction(nameof(PostController.Index));
                }
            }

            postForSavingPost.Title = formData.Title;
            postForSavingPost.Body = formData.Body;
            postForSavingPost.Published = formData.Published;


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
            model.Published = post.Published;

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
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

            var Post = DbContext.Posts.FirstOrDefault(p => p.Id == id);

            if (Post != null)
            {
                DbContext.Comments.RemoveRange(Post.Comments);
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
            var isAdmin = User.IsInRole("Admin");

            if (isAdmin)
            {
                postForDetail = DbContext.Posts.FirstOrDefault(p =>
                    p.Id == id.Value);
            }
            else
            {
                postForDetail = DbContext.Posts.FirstOrDefault(p =>
                    p.Published && p.Id == id.Value);
            }


            if (postForDetail == null)
                return RedirectToAction(nameof(PostController.Index));

            var model = new DetailPostViewModel();
            model.Title = postForDetail.Title;
            model.Body = postForDetail.Body;
            model.MediaUrl = postForDetail.MediaUrl;

            return View(model);
        }

        [HttpGet]
        [Route("blog/{slug}")]
        public ActionResult DetailBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return RedirectToAction(nameof(PostController.Index));
            }
            var appUserId = User.Identity.GetUserId();
            var isAdmin = User.IsInRole("Admin");

            if (isAdmin)
            {
                postForDetail = DbContext.Posts.FirstOrDefault(p =>
                    p.Slug == slug);
            }
            else
            {
                postForDetail = DbContext.Posts.FirstOrDefault(p =>
                    p.Published && p.Slug == slug);
            }

            if (postForDetail == null)
                return RedirectToAction(nameof(PostController.Index));

            var model = new DetailPostViewModel();
            model.Title = postForDetail.Title;
            model.Body = postForDetail.Body;
            model.MediaUrl = postForDetail.MediaUrl;
            model.Comments = postForDetail.Comments;

            return View("Detail", model);
        }

        [HttpPost]
        [Route("blog/{slug}")]
        public ActionResult DetailBySlug(string slug, CreateCommentViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(PostController.DetailBySlug), "Post", new { slug = slug });
            }

            var appUserEm = User.Identity.GetUserName();
            var appUserId = User.Identity.GetUserId();

            postForDetail = DbContext.Posts.FirstOrDefault(p => p.Slug == slug);

            commentForSaving = new Comment();
            commentForSaving.DateCreated = DateTime.Now;
            commentForSaving.UserEmail = appUserEm;
            commentForSaving.UserId = appUserId;
            commentForSaving.PostId = postForDetail.Id;
            commentForSaving.Body = formData.Body;

            if (commentForSaving == null)
            {
                return RedirectToAction(nameof(CommentController.Index), nameof(CommentController).Substring(0, 7));
            }

            DbContext.Comments.Add(commentForSaving);

            DbContext.SaveChanges();

            return RedirectToAction(nameof(PostController.DetailBySlug), new { slug = slug });


        }
    }
}
