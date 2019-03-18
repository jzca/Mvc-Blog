﻿using Microsoft.AspNet.Identity;
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
                .Where(p => p.UserId == appUserId)
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
        public ActionResult DetailBySlug(string slug, CreateEditCommentViewModel formData)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var appUserEm = User.Identity.GetUserName();
            var appUserId = User.Identity.GetUserId();

            //if (DbContext.Comments.Any(p => p.UserEmail == appUserEm &&
            //p.Body == formData.Body &&
            //(!id.HasValue || p.Id != id.Value)))
            //{
            //    ModelState.AddModelError(nameof(CreateEditCommentViewModel.Body),
            //        "You have already commented this text.");

            //    return View();
            //}

            postForDetail = DbContext.Posts.FirstOrDefault(p=> p.Slug == slug);



            //if (commentForSaving.Post.Id != null)
            //{
            //}
            //else
            //{
            //   // commentForSaving = DbContext.Comments.FirstOrDefault(
            //   //p => p.Id == id && p.UserEmail == appUserEm);

            //   // if (commentForSaving == null)
            //   // {
            //   //     return RedirectToAction(nameof(CommentController.Index));
            //   // }
            //}

            commentForSaving = new Comment();
            commentForSaving.DateCreated = DateTime.Now;
            commentForSaving.UserEmail = appUserEm;
            commentForSaving.UserId = appUserId;
            commentForSaving.PostId= postForDetail.Id;
            commentForSaving.Body = formData.Body;
            DbContext.Comments.Add(commentForSaving);
            postForDetail.Comments.Add(commentForSaving);

            //commentForSaving.ReasonUpdated = formData.ReasonUpdated;
            //commentForSaving.DateUpdated = DateTime.Now;


            DbContext.SaveChanges();

            return RedirectToAction(nameof(CommentController.Index));


        }
    }
}
