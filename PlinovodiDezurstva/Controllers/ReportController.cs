using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using PlinovodiDezurstva.Data;
using PlinovodiDezurstva.Infrastructure;
using PlinovodiDezurstva.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using PlinovodiDezurstva.Utils;

namespace PlinovodiDezurstva.Controllers
{
    [SessionExpireFilterAttribute]
    public class ReportController : Controller
    {

        private readonly IServiceProvider _services;
        private readonly IPlinovodiDutyDataRead _plinovodiDutyDataRead;
        private readonly ILogger _logger;

        public ReportController(IServiceProvider services, IPlinovodiDutyDataRead plinovodiDutyDataRead, ILogger logger)
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

                ISession session = _services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;
                SessionLogIn ses = session.GetJson<SessionLogIn>("ses");

                IEnumerable<Duty> dutyList = await _plinovodiDutyDataRead.GetEmployeeDuties(ses.EmployeeId);

                this._logger.Information($"End {nameof(Index)}");
                return View(dutyList);
            }
            catch (Exception ex)
            {
                this._logger.Error($"Error {nameof(Index)} {ex.Message} {ex.StackTrace}");
                throw ex;
            }
        }

        [HttpPost]
        public async Task<FileResult> Report(int Id)
        {
            try
            {
                this._logger.Information($"Start {nameof(Report)} Id = {Id}");

                ISession session = _services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;
                SessionLogIn ses = session.GetJson<SessionLogIn>("ses");

                Employee employee = await _plinovodiDutyDataRead.GetEmployee(ses.EmployeeId);
                Duty duty = await _plinovodiDutyDataRead.GetDuty(Id);
                IEnumerable<Intervention> interventionList = await _plinovodiDutyDataRead.GetInterventions(Id);

                var stream = PlinovodiDezurstvaUtils.GenerateReport(employee, duty, interventionList);

                Response.Headers.Append("content-disposition", "inline; filename=file.pdf");

                this._logger.Information($"End {nameof(Report)}");
                return File(stream.ToArray(), "application/pdf", $"{employee.Name}{employee.Surname}{duty.From.ToString("d.M.yy")}.pdf");
            }
            catch (Exception ex)
            {
                this._logger.Error($"Error {nameof(Report)} {ex.Message} {ex.StackTrace}");
                throw ex;
            }
        }
    }
}