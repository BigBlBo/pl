using Microsoft.AspNetCore.Mvc;
using PlinovodiDezurstva.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace PlinovodiDezurstva.Controllers
{
    public class HomeController : Controller
    {
        private readonly IServiceProvider _services;
        public HomeController(IServiceProvider services)
        {
            this._services = services;
        }

        public ViewResult Index()
        {
            LoginModel lm = new LoginModel();
            lm.DezurniModel = new List<Dezurni>();
            lm.DezurniModel.Add(new Dezurni { Id = 1, ImePriimek = "Bozo" });
            lm.DezurniModel.Add(new Dezurni { Id = 2, ImePriimek = "Joco" });
            return View("Login", lm);
        }

        [HttpPost]
        public ActionResult GetIntervalByDezurniId(int dezurniid)
        {
            List<Interval> objcity = new List<Interval>();
            Random rnd = new Random();
            int length = 20;
            var str = "";
            for (var i = 0; i < length; i++)
            {
                str += ((char)(rnd.Next(1, 26) + 64)).ToString();
            }

            objcity.Add(new Interval { Id = 1, Obdobje = str });
            SelectList obgcity = new SelectList(objcity, "Id", "Obdobje", 0);
            return Json(obgcity);
        }

        [HttpPost]
        public RedirectToActionResult LogIn(int DezurniModel, int ddlcity)
        {
            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;
            session.SetInt32("IdDezurni", DezurniModel);
            session.SetInt32("ddlcity", ddlcity);

            return RedirectToAction("Index", "Duty");
         }
    }
}
