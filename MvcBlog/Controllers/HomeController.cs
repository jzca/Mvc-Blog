using Microsoft.AspNet.Identity;
using MvcBlog.Models;
using MvcBlog.Models.Domain;
using MvcBlog.Models.ViewModels;
using PostDatabase.Controllers;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcBlog.Controllers
{
    public class HomeController : Controller
    {
        private ApplicationDbContext DbContext;
        private List<IndexHomeViewModel> modelGlobal;

        public HomeController()
        {
            DbContext = new ApplicationDbContext();
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Your home page.";

            modelGlobal = DbContext.Posts
                .Where(p => p.Published)
                .OrderByDescending(p => p.DateCreated)
                .Select(p => new IndexHomeViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Body = p.Body,
                    DateCreated = p.DateCreated,
                    MediaUrl = p.MediaUrl,
                    Slug = p.Slug,
                    AmtComment = p.Comments.Count()
                }).ToList();

            return View(modelGlobal);
        }

        public ActionResult Search(string text)
        {
            if (User.IsInRole("Admin"))
            {
                modelGlobal = DbContext.Posts
                    .Where(p => 
                    p.Title.Contains(text) || p.Slug.Contains(text) || p.Body.Contains(text))
                    .OrderByDescending(p => p.DateCreated)
                    .Select(p => new IndexHomeViewModel
                    {
                        Id = p.Id,
                        Title = p.Title,
                        Body = p.Body,
                        DateCreated = p.DateCreated,
                        MediaUrl = p.MediaUrl,
                        Slug = p.Slug,
                    }).ToList();
            }
            else
            {
                modelGlobal = DbContext.Posts
                     .Where(p => p.Published &&
                     (p.Title.Contains(text) || p.Slug.Contains(text) || p.Body.Contains(text)))
                     .OrderByDescending(p => p.DateCreated)
                     .Select(p => new IndexHomeViewModel
                     {
                         Id = p.Id,
                         Title = p.Title,
                         Body = p.Body,
                         DateCreated = p.DateCreated,
                         MediaUrl = p.MediaUrl,
                         Slug = p.Slug
                     }).ToList();
            }
            if (!modelGlobal.Any())
            {
                return View();
            }



            return View(nameof(HomeController.Index), modelGlobal);
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            //var eService = new MvcBlog.Models.EmailService();
            //eService.Send("test-to@test.com", "this is the body", "subject is CS 2");

            return View();
        }

    }
}