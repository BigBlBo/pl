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
using Serilog;

namespace PlinovodiDezurstva.Controllers
{
    public class HomeController : Controller
    {
        private readonly IServiceProvider _services;
        private readonly IPlinovodiDutyDataRead _plinovodiDutyDataRead;
        private readonly ILogger _logger;

        public HomeController(IServiceProvider services, IPlinovodiDutyDataRead plinovodiDutyDataRead, ILogger logger)
        {
            this._services = services;
            this._plinovodiDutyDataRead = plinovodiDutyDataRead;
            this._logger = logger;
        }

        public async Task<ViewResult> Index()
        {
            try
            {
                this._logger.Information($"Start {nameof(Index)}");

                IEnumerable<Employee> employeeList = await _plinovodiDutyDataRead.GetEmployees();
                LoginModel loginModel = new LoginModel();

                foreach (Employee employee in employeeList)
                {
                    loginModel.DezurniModel.Add(new Dezurni { Id = employee.Id, ImePriimek = employee.Name + " " + employee.Surname });
                }

                this._logger.Information($"End {nameof(Index)}");
                return View("Login", loginModel);
            }
            catch(Exception ex)
            {
                this._logger.Error($"Error {nameof(Index)} {ex.Message} {ex.StackTrace}");
                throw ex;
            }
        }

        public async Task<ActionResult> GetIntervalByEmployeeId(int employeeid)
        {
            try
            {
                this._logger.Information($"Start {nameof(GetIntervalByEmployeeId)} employeeid = {employeeid}");

                IEnumerable<Duty> dutyList = await _plinovodiDutyDataRead.GetEmployeeDuties(employeeid);

                List<Interval> dutyInterval = new List<Interval>();
                foreach (Duty duty in dutyList)
                {
                    dutyInterval.Add(new Interval { Id = duty.Id, Obdobje = duty.From.ToString("d.M.yyyy HH:mm") + " - " + duty.To.ToString("d.M.yyyy HH:mm"), Disabled = DateTime.Now < duty.From });
                }

                this._logger.Information($"End {nameof(GetIntervalByEmployeeId)}");
                return Json(dutyInterval);
            }
            catch (Exception ex)
            {
                this._logger.Error($"Error {nameof(GetIntervalByEmployeeId)} {ex.Message} {ex.StackTrace}");
                throw ex;
            }
        }

        [HttpPost]
        public async Task<RedirectToActionResult> LogIn(int employeeId, int dutyId)
        {
            try
            {
                this._logger.Information($"Start {nameof(LogIn)} employeeId = {employeeId} dutyId = {dutyId}");

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

                this._logger.Information($"End {nameof(LogIn)}");
                return RedirectToAction("Index", "Duty");
            }
            catch (Exception ex)
            {
                this._logger.Error($"Error {nameof(LogIn)} {ex.Message} {ex.StackTrace}");
                throw ex;
            }
        }

        public RedirectToActionResult LogOut()
        {
            try
            {
                this._logger.Information($"Start {nameof(LogOut)}");

                ISession session = _services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;
                session.Clear();

                this._logger.Information($"End {nameof(LogOut)}");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                this._logger.Error($"Error {nameof(LogOut)} {ex.Message} {ex.StackTrace}");
                throw ex;
            }
        }
    }
}