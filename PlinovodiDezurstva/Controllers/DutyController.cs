using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using PlinovodiDezurstva.Data;
using PlinovodiDezurstva.Infrastructure;
using PlinovodiDezurstva.Models;
using PlinovodiDezurstva.Models.ViewModels;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlinovodiDezurstva.Controllers
{
    [SessionExpireFilterAttribute]
    public class DutyController : Controller
    {
        private readonly IServiceProvider _services;
        private readonly IPlinovodiDutyDataRead _plinovodiDutyDataRead;
        private readonly IPlinovodiDutyDataWrite _plinovodiDutyDataWrite;
        private readonly ILogger _logger;

        public DutyController(IServiceProvider services, IPlinovodiDutyDataRead plinovodiDutyDataRead, 
                IPlinovodiDutyDataWrite plinovodiDutyDataWrite, ILogger logger)
        {
            this._services = services;
            this._plinovodiDutyDataRead = plinovodiDutyDataRead;
            this._plinovodiDutyDataWrite = plinovodiDutyDataWrite;
            this._logger = logger;
        }

        public async Task<ViewResult> Index()
        {
            this._logger.Information($"Start {nameof(Index)}");

            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;
            SessionLogIn ses = session.GetJson<SessionLogIn>("ses");
            IEnumerable<Intervention> interventionList = await _plinovodiDutyDataRead.GetInterventions(ses.DutyId);

            this._logger.Information($"End {nameof(Index)}");
            return View(interventionList);
        }

        public ViewResult CreateIntervention()
        {
            this._logger.Information($"Start {nameof(CreateIntervention)}");

            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;
            SessionLogIn ses = session.GetJson<SessionLogIn>("ses");

            InterventionEdit interventionEdit = new InterventionEdit();
            for(int index = 0; index < ses.DaysOfDuty.Count; index++)
            {
                interventionEdit.Days.Add(new InterventionDay { Id = index, Day = ses.DaysOfDuty[index].ToString("dd-MM-yyyy") });
            }

            this._logger.Information($"End {nameof(CreateIntervention)}");
            return View("AddEditIntervention", interventionEdit);
        }

        public async Task<ViewResult> EditIntervention(int Id)
        {
            this._logger.Information($"Start {nameof(EditIntervention)} Id = {Id}");

            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;
            SessionLogIn ses = session.GetJson<SessionLogIn>("ses");
            Intervention intervention = await _plinovodiDutyDataRead.GetIntervention(Id);

            InterventionEdit interventionEdit = new InterventionEdit();
            interventionEdit.Id = intervention.Id;
            interventionEdit.ShortDescription = intervention.ShortDescription;
            interventionEdit.LongDescription = intervention.LongDescription;
            for (int index = 0; index < ses.DaysOfDuty.Count; index++)
            {
                interventionEdit.Days.Add(new InterventionDay { Id = index, Day = ses.DaysOfDuty[index].ToString() });
            }

            for(int index = 0; index < ses.DaysOfDuty.Count;index++)
            {
                if (ses.DaysOfDuty[index].Day == intervention.From.Day)
                {
                    interventionEdit.SelectedDay = index;
                    break;
                }
            }

            for (int index = 0; index <= 23; index++)
            {
                if (intervention.From.Hour == index)
                {
                    interventionEdit.SelectedStartTime = index;
                    break;
                }
            }

            for (int index = 1; index <= 24; index++)
            {
                if (intervention.To.Hour == index)
                {
                    interventionEdit.SelectedEndTime = index;
                    break;
                }
            }

            this._logger.Information($"End {nameof(EditIntervention)}");
            return View("AddEditIntervention", interventionEdit);
        }

        [HttpPost]
        public async Task<RedirectToActionResult> EditIntervention(InterventionPostBack interventionPostBack)
        {
            this._logger.Information($"Start {nameof(EditIntervention)} interventionPostBack = {interventionPostBack}");

            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;
            SessionLogIn ses = session.GetJson<SessionLogIn>("ses");
            Intervention intervention = new Intervention();
            intervention.Id = interventionPostBack.Id;
            intervention.DutyId = ses.DutyId;
            intervention.From = ses.DaysOfDuty[interventionPostBack.InterventionDayId];
            intervention.From = new DateTime(intervention.From.Year, intervention.From.Month, intervention.From.Day, interventionPostBack.InterventionTimeStartId, 0, 0);

            intervention.To = ses.DaysOfDuty[interventionPostBack.InterventionDayId];
            intervention.To = new DateTime(intervention.To.Year, intervention.To.Month, intervention.To.Day, interventionPostBack.InterventionTimeEndId, 0, 0);

            intervention.ShortDescription = interventionPostBack.ShortDescription;
            intervention.LongDescription = interventionPostBack.LongDescription != null ? interventionPostBack.LongDescription : "";

            if (interventionPostBack.Id == 0)
            {
                await _plinovodiDutyDataWrite.InsertIntervention(intervention);
            }
            else
            {
                await _plinovodiDutyDataWrite.UpdateIntervention(intervention);
            }

            this._logger.Information($"End {nameof(EditIntervention)}");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<RedirectToActionResult> Delete(int Id)
        {
            this._logger.Information($"Start {nameof(Delete)} Id = {Id}");

            Intervention intervention = new Intervention();
            intervention.Id = Id;

            await _plinovodiDutyDataWrite.DeleteIntervention(intervention);

            this._logger.Information($"End {nameof(Delete)}");
            return RedirectToAction("Index");
        }
    }
}