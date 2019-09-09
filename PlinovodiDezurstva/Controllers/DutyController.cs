using Microsoft.AspNetCore.Mvc;
using PlinovodiDezurstva.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlinovodiDezurstva.Controllers
{
    public class DutyController : Controller
    {
        public ViewResult Index()
        {
            return View();
        }

        public ViewResult CreateIntervention()
        {
            return View("EditIntervention", new Intervention());
        }

        public ViewResult EditIntervention()
        {
            return View("Edit", new Intervention());
        }

        [HttpPost]
        public IActionResult Delete(int productId)
        {

            return RedirectToAction("Index");
        }
    }
}
