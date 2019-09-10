using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using PlinovodiDezurstva.Data;
using PlinovodiDezurstva.Infrastructure;
using PlinovodiDezurstva.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Serilog;

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
            this._logger.Information($"Start {nameof(Index)}");

            ISession session = _services.GetRequiredService<IHttpContextAccessor>()?.HttpContext.Session;
            SessionLogIn ses = session.GetJson<SessionLogIn>("ses");

            IEnumerable<Duty> dutyList = await _plinovodiDutyDataRead.GetEmployeeDuties(ses.EmployeeId);

            this._logger.Information($"End {nameof(Index)}");
            return View(dutyList);
        }

        [HttpPost]
        public async Task<FileResult> Report(int Id)
        {
            this._logger.Information($"Start {nameof(Report)} Id = {Id}");

            Duty duty = await _plinovodiDutyDataRead.GetDuty(Id);
            IEnumerable<Intervention> interventionList = await _plinovodiDutyDataRead.GetInterventions(Id);

            int[] takenHoursInOneDay = new int[8];
            Array.Clear(takenHoursInOneDay, 0, takenHoursInOneDay.Length);


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

                if(indexDays != 6 && indexDays != 7)
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

            var stream = new MemoryStream();
            Document pdfDoc = new Document(PageSize.A4, 25, 25, 25, 15);
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, stream);
            pdfDoc.Open();

            Chunk chunk = new Chunk("Your Credit Card Statement Report has been Generated", FontFactory.GetFont("Arial", 20, Font.BOLDITALIC, BaseColor.MAGENTA));
            pdfDoc.Add(chunk);
            Paragraph line = new Paragraph(new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
            pdfDoc.Add(line);

            //Table
            PdfPTable  table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 20f;
            table.SpacingAfter = 30f;

            //Cell
            PdfPCell  cell = new PdfPCell();
            chunk = new Chunk("This Month's Transactions of your Credit Card");
            cell.AddElement(chunk);
            cell.Colspan = 5;
            cell.BackgroundColor = BaseColor.PINK;
            table.AddCell(cell);

            table.AddCell("S.No");
            table.AddCell("NYC Junction");
            table.AddCell("Item");
            table.AddCell("Cost");
            table.AddCell("Date");

            table.AddCell("1");
            table.AddCell("David Food Store");
            table.AddCell("Fruits & Vegetables");
            table.AddCell("$100.00");
            table.AddCell("June 1");

            table.AddCell("2");
            table.AddCell("Child Store");
            table.AddCell("Diaper Pack");
            table.AddCell("$6.00");
            table.AddCell("June 9");

            table.AddCell("3");
            table.AddCell("Punjabi Restaurant");
            table.AddCell("Dinner");
            table.AddCell("$29.00");
            table.AddCell("June 15");

            table.AddCell("4");
            table.AddCell("Wallmart Albany");
            table.AddCell("Grocery");
            table.AddCell("$299.50");
            table.AddCell("June 25");

            table.AddCell("5");
            table.AddCell("Singh Drugs");
            table.AddCell("Back Pain Tablets");
            table.AddCell("$14.99");
            table.AddCell("June 28");

            //Add table to document
            pdfDoc.Add(table);
            pdfWriter.CloseStream = false;
            pdfDoc.Close();

            Response.Headers.Append("content-disposition", "inline; filename=file.pdf");

            this._logger.Information($"End {nameof(Index)}");
            return File(stream.ToArray(), "application/pdf", "ImageExport.pdf");
        }
    }
}
