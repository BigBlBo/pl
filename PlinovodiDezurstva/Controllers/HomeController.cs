using Microsoft.AspNetCore.Mvc;
using PlinovodiDezurstva.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using PlinovodiDezurstva.Models;
using PlinovodiDezurstva.Infrastructure;
using PlinovodiDezurstva.Data;

namespace PlinovodiDezurstva.Controllers
{
    public class HomeController : Controller
    {
        private readonly IServiceProvider _services;
        private readonly IPlinovodiDutyDataRead _plinovodiDutyDataRead;

        public HomeController(IServiceProvider services, IPlinovodiDutyDataRead plinovodiDutyDataRead)
        {
            this._services = services;
            this._plinovodiDutyDataRead = plinovodiDutyDataRead;
        }

        public async Task<ViewResult> Index()
        {
            IEnumerable<Employee> employeeList = await _plinovodiDutyDataRead.GetEmployee();
            LoginModel loginModel = new LoginModel();

            foreach(Employee employee in employeeList)
            {
                loginModel.DezurniModel.Add(new Dezurni { Id = employee.Id, ImePriimek = employee.Name + " " + employee.Surname });
            }

            return View("Login", loginModel);
        }

        public async Task<ActionResult> GetIntervalByEmployeeId(int dezurniid)
        {
            IEnumerable<Duty> dutyList = await _plinovodiDutyDataRead.GetEmployeeDuty(dezurniid);

            List<Interval> dutyInterval = new List<Interval>();
            foreach (Duty duty in dutyList)
            {
                dutyInterval.Add(new Interval { Id = duty.Id, Obdobje = duty.From.ToString() + " - " + duty.To.ToString() });
            }

            return Json(dutyInterval);
        }

        [HttpPost]
        public async Task<RedirectToActionResult> LogIn(int employeeId, int dutyId)
        {
            Duty duty = await _plinovodiDutyDataRead.GetDuty(dutyId);

            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;
            SessionLogIn ses = new SessionLogIn();
            ses.EmployeeId = employeeId;
            ses.DutyId = dutyId;
            DateTime day = duty.From;
            ses.DaysOfDuty.Add(day);

            for (int i = 0; i < 7; i++)
            {
                day = day.AddDays(1);
                ses.DaysOfDuty.Add(day);
            }

            session.SetJson("ses", ses);

            return RedirectToAction("Index", "Duty");
         }

        public RedirectToActionResult LogOut()
        {
            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;
            session.Clear();

            return RedirectToAction("Index");
        }
    }
}
