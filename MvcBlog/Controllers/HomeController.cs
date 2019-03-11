using Microsoft.AspNet.Identity;
using MvcBlog.Models;
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

        public HomeController()
        {
            DbContext = new ApplicationDbContext();
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Your home page.";

            var appUserId = User.Identity.GetUserId();

            var model = DbContext.Posts
                .Where(p => p.UserId == appUserId)
                .Select(p => new IndexHomeViewModel
                {
                    Id = p.Id,
                    Title = p.Title,
                    Body = p.Body,
                    DateCreated=p.DateCreated,
                    MediaUrl=p.MediaUrl
                }).ToList();

            return View(model);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Single()
        {
            ViewBag.Message = "Your blog page.";
            return View();
        }

    }
}