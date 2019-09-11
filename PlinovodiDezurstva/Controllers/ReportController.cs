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
                IList<Intervention> interventionListCopy = new List<Intervention>();

                int[] takenHoursInOneDay = new int[8];
                Array.Clear(takenHoursInOneDay, 0, takenHoursInOneDay.Length);
                for (int indexDays = 0; indexDays < 8; indexDays++) //split interval if within work hours
                {
                    foreach (Intervention intervention in interventionList)
                    {
                        if (intervention.From.Day == duty.From.AddDays(indexDays).Day && indexDays < 5)
                        {
                            if(intervention.From.Hour < 7)
                            {
                                Intervention interventionCopy = intervention.GetCopy();
                                interventionCopy.To = new DateTime(interventionCopy.To.Year, interventionCopy.To.Month, interventionCopy.To.Day,
                                    interventionCopy.To.Hour > 7 ? 7 : interventionCopy.To.Hour, interventionCopy.To.Minute, interventionCopy.To.Second);
                                interventionListCopy.Add(interventionCopy);
                            }

                            if (intervention.To.Hour > 15)
                            {
                                Intervention interventionCopy = intervention.GetCopy();
                                interventionCopy.From = new DateTime(interventionCopy.From.Year, interventionCopy.From.Month, interventionCopy.From.Day,
                                    interventionCopy.From.Hour < 15 ? 15 : interventionCopy.From.Hour, interventionCopy.From.Minute, interventionCopy.From.Second);
                                interventionListCopy.Add(interventionCopy);
                            }
                        }
                        if (intervention.From.Day == duty.From.AddDays(indexDays).Day && indexDays >= 5)
                        {
                            interventionListCopy.Add(intervention);
                        }
                    }
                }


                interventionList = interventionListCopy;
                int[] hoursTaken = new int[24];
                for (int indexDays = 0; indexDays < 8; indexDays++)
                {
                    Array.Clear(hoursTaken, 0, hoursTaken.Length);
                    foreach(Intervention intervention in interventionList)
                    {
                        if(intervention.From.Day == duty.From.AddDays(indexDays).Day)
                        {
                            for(int hour = intervention.From.Hour; hour < intervention.To.Hour; hour++)
                            {
                                hoursTaken[hour]++;
                            }
                        }
                    }

                    if(indexDays != 5 && indexDays != 6)
                    {
                        for (int hour = 6; hour < 14; hour++)
                        {
                            hoursTaken[hour] = 0;
                        }
                    }

                    for (int indexHours = 0; indexHours < 24; indexHours++)
                    {
                        if(hoursTaken[indexHours] > 0)
                        {
                            takenHoursInOneDay[indexDays]++;
                        }
                    }
                }

                var stream = PlinovodiDezurstvaUtils.GetPregeledUrReport(employee, duty, takenHoursInOneDay, interventionList);

                Response.Headers.Append("content-disposition", "inline; filename=file.pdf");

                this._logger.Information($"End {nameof(Report)}");
                return File(stream.ToArray(), "application/pdf", "ImageExport.pdf");
            }
            catch (Exception ex)
            {
                this._logger.Error($"Error {nameof(Report)} {ex.Message} {ex.StackTrace}");
                throw ex;
            }
        }
    }
}