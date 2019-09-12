using iTextSharp.text;
using iTextSharp.text.pdf;
using PlinovodiDezurstva.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlinovodiDezurstva.Utils
{
    public class PlinovodiDezurstvaUtils
    {
        public static MemoryStream GenerateReport(Employee employee, Duty duty, IEnumerable<Intervention> interventionList)
        {
            var stream = new MemoryStream();

            Document pdfDoc = new Document(PageSize.A4, 25, 25, 25, 15);
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, stream);
            pdfDoc.Open();

            IEnumerable<Intervention> interventionListSeperated = SeperateInterventions(interventionList, duty);
            int[] takenHoursInOneDay = GetTakenHours(interventionListSeperated, duty);
            GenerateHourOverwiev(pdfDoc, employee, duty, takenHoursInOneDay, interventionListSeperated);
            GenerateComplains(pdfDoc, employee, interventionList);

            pdfWriter.CloseStream = false;
            pdfDoc.Close();

            return stream;
        }

        private static void GenerateHourOverwiev(Document pdfDoc, Employee employee, Duty duty, int[] takenHoursInOneDay,
            IEnumerable<Intervention> interventionListSeperated)
        {
            double realHoursAllSum = 0;
            int hoursSum = 0;
            double realHoursSum = 0;

            //process header
            PlinovodiDezurstvaUtils.AddHeader(pdfDoc, employee, duty);

            //process workdays
            PdfPTable table = PlinovodiDezurstvaUtils.GetTable(5);
            PlinovodiDezurstvaUtils.AddTableLine(table, new List<string>() { "Datum", "Dan", "", "Število ur", "Število rednih  ur (10%)" });

            for (int indexDays = 0; indexDays < 8; indexDays++)
            {
                if (indexDays == 5 || indexDays == 6) { continue; }

                int hours = PlinovodiDezurstvaUtils.GetSteviloUr(indexDays, takenHoursInOneDay);
                PlinovodiDezurstvaUtils.AddTableLine(table, new List<string>() {
                        PlinovodiDezurstvaUtils.SubStringMonthInDate(duty.From.AddDays(indexDays).ToString("d.MMMM.yy")),
                        PlinovodiDezurstvaUtils.GetDayName(indexDays),
                        "", hours.ToString(), (hours * 0.1).ToString() });
                hoursSum += hours; realHoursSum += hours * 0.1;
            }

            PlinovodiDezurstvaUtils.AddTableLine(table, new List<string>() { "", "", "Skupaj", hoursSum.ToString(), realHoursSum.ToString() });
            pdfDoc.Add(table);
            realHoursAllSum += realHoursSum;
            /////////////////////////////////////////////////

            //process weekends
            hoursSum = 0;
            realHoursSum = 0;
            table = PlinovodiDezurstvaUtils.GetTable(5);
            PlinovodiDezurstvaUtils.AddTableLine(table, new List<string>() { "Datum", "Dan", "", "Število ur", "Število rednih  ur (20%)" });

            for (int indexDays = 0; indexDays < 8; indexDays++)
            {
                if (indexDays != 5 && indexDays != 6) { continue; }

                int hours = PlinovodiDezurstvaUtils.GetSteviloUr(indexDays, takenHoursInOneDay);
                PlinovodiDezurstvaUtils.AddTableLine(table, new List<string>() {
                        PlinovodiDezurstvaUtils.SubStringMonthInDate(duty.From.AddDays(indexDays).ToString("d.MMMM.yy")),
                        PlinovodiDezurstvaUtils.GetDayName(indexDays),
                        "", hours.ToString(), (hours * 0.2).ToString() });

                hoursSum += hours; realHoursSum += hours * 0.2;
            }

            PlinovodiDezurstvaUtils.AddTableLine(table, new List<string>() { "", "", "Skupaj", hoursSum.ToString(), realHoursSum.ToString() });
            pdfDoc.Add(table);
            realHoursAllSum += realHoursSum;
            /////////////////////////////////////

            //process interventions
            table = PlinovodiDezurstvaUtils.GetTable(5);
            PlinovodiDezurstvaUtils.AddTableLine(table, new List<string>() { "Datum", "Objekt", "Od ure", "Do ure", "Opis del" });
            bool weHaveInterventions = false;
            foreach (Intervention intervention in interventionListSeperated)
            {
                int endHour = intervention.From.Day != intervention.To.Day ? 24 : intervention.To.Hour;
                PlinovodiDezurstvaUtils.AddTableLine(table, new List<string>() { PlinovodiDezurstvaUtils.SubStringMonthInDate(intervention.From.ToString("d.MMMM.yy")), "",
                        intervention.From.ToString("HH:mm"), endHour + ":00", intervention.ShortDescription });

                weHaveInterventions = true;
            }
            if (weHaveInterventions)
            {
                PdfPTable pt = new PdfPTable(1);
                PlinovodiDezurstvaUtils.AddCellCenterBold(pt, "INTERVENCIJA", 12);
                pdfDoc.Add(pt);

                pdfDoc.Add(table);
            }
            //////////////
            ///

            //add Footer
            PlinovodiDezurstvaUtils.AddFooter(pdfDoc, employee, duty, realHoursAllSum, takenHoursInOneDay);
        }

        private static void GenerateComplains(Document pdfDoc, Employee employee, IEnumerable<Intervention> interventionList)
        {
            foreach (Intervention intervention in interventionList)
            {
                pdfDoc.NewPage();
                PdfPTable pt = new PdfPTable(1);
                PlinovodiDezurstvaUtils.AddCellCenterBold(pt, "PORO\u010cILO O INTERVENCIJI", 18);
                pdfDoc.Add(pt);

                pdfDoc.Add(Chunk.NEWLINE);
                pdfDoc.Add(Chunk.NEWLINE);

                PdfPTable table = GetTable(8);
                //row1
                PdfPCell cell = new PdfPCell(new Phrase("Ime in Priimek: "));
                cell.Colspan = 2;
                cell.Border = 0;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(employee.Name + " " + employee.Surname));
                cell.Colspan = 6;
                cell.Border = 0;
                cell.BorderWidthBottom = 1;
                table.AddCell(cell);
                //row2
                cell = new PdfPCell(new Phrase("Nedovoljeno stanje prijavil: "));
                cell.Colspan = 3;
                cell.Border = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(""));
                cell.Colspan = 5;
                cell.Border = 0;
                cell.BorderWidthBottom = 1;
                table.AddCell(cell);
                //row3
                cell = new PdfPCell(new Phrase("Prijava nedovoljenega stanja ob: "));
                cell.Colspan = 3;
                cell.Border = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(intervention.From.ToString("HH:mm")));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Border = 0;
                cell.BorderWidthBottom = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("uri"));
                cell.Colspan = 1;
                cell.Border = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Objekt / plinovod: "));
                cell.Colspan = 2;
                cell.Border = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(""));
                cell.Colspan = 1;
                cell.Border = 0;
                cell.BorderWidthBottom = 1;
                table.AddCell(cell);

                //row4
                cell = new PdfPCell(new Phrase("Prihod na objekt ob: "));
                cell.Colspan = 2;
                cell.Border = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(""));
                cell.Colspan = 1;
                cell.Border = 0;
                cell.BorderWidthBottom = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("uri"));
                cell.Colspan = 1;
                cell.Border = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Odhod z objekta ob: "));
                cell.Colspan = 2;
                cell.Border = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(""));
                cell.Colspan = 1;
                cell.Border = 0;
                cell.BorderWidthBottom = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("uri"));
                cell.HorizontalAlignment = Element.ALIGN_RIGHT;
                cell.Colspan = 1;
                cell.Border = 0;
                table.AddCell(cell);

                //row4
                cell = new PdfPCell(new Phrase("Zaključek ob:: "));
                cell.Colspan = 2;
                cell.Border = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(intervention.To.ToString("HH:mm")));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 1;
                cell.Border = 0;
                cell.BorderWidthBottom = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("uri"));
                cell.Colspan = 1;
                cell.Border = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Skupaj število ur: "));
                cell.Colspan = 2;
                cell.Border = 0;
                table.AddCell(cell);
                int endHour = intervention.From.Day != intervention.To.Day ? 24 : intervention.To.Hour;
                cell = new PdfPCell(new Phrase((endHour - intervention.From.Hour).ToString()));
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.Colspan = 2;
                cell.Border = 0;
                cell.BorderWidthBottom = 1;
                table.AddCell(cell);



                pdfDoc.Add(table);

                Chunk chunk = new Chunk("Kratek opis javljenega nedovoljenega stanja:\n", FontFactory.GetFont(BaseFont.TIMES_ROMAN, 14, Font.BOLD, BaseColor.BLACK));
                pdfDoc.Add(chunk);
                table = new PdfPTable(1);
                cell = new PdfPCell(new Phrase(intervention.ShortDescription));
                cell.Colspan = 2;
                cell.Border = 0;
                table.AddCell(cell);
                pdfDoc.Add(table);

                chunk = new Chunk("Stanje ob prihodu:\n", FontFactory.GetFont(BaseFont.TIMES_ROMAN, 14, Font.BOLD, BaseColor.BLACK));
                pdfDoc.Add(chunk);
                table = new PdfPTable(1);
                cell = new PdfPCell(new Phrase("Opis stanja ob prihodu."));
                cell.Colspan = 2;
                cell.Border = 0;
                table.AddCell(cell);
                pdfDoc.Add(table);

                chunk = new Chunk("Ukrepi in opis opravljenega dela:\n", FontFactory.GetFont(BaseFont.TIMES_ROMAN, 14, Font.BOLD, BaseColor.BLACK));
                pdfDoc.Add(chunk);
                table = new PdfPTable(1);
                cell = new PdfPCell(new Phrase(intervention.LongDescription));
                cell.Colspan = 2;
                cell.Border = 0;
                table.AddCell(cell);
                pdfDoc.Add(table);

                chunk = new Chunk("Vzrok:\n", FontFactory.GetFont(BaseFont.TIMES_ROMAN, 14, Font.BOLD, BaseColor.BLACK));
                pdfDoc.Add(chunk);
                table = new PdfPTable(1);
                cell = new PdfPCell(new Phrase("Opis vzroka."));
                cell.Colspan = 2;
                cell.Border = 0;
                table.AddCell(cell);
                pdfDoc.Add(table);

                chunk = new Chunk("Porabljen material:", FontFactory.GetFont(BaseFont.TIMES_ROMAN, 14, Font.BOLD, BaseColor.BLACK));
                pdfDoc.Add(chunk);
                table = GetTable(2);
                AddTableLine(table, new List<string> { "Naziv / ime materiala", "Količina" });
                AddTableLine(table, new List<string> { " ", " " });
                AddTableLine(table, new List<string> { " ", " " });
                AddTableLine(table, new List<string> { " ", " " });
                AddTableLine(table, new List<string> { " ", " " });
                pdfDoc.Add(table);

                table = GetTable(8);
                cell = new PdfPCell(new Phrase("Datum intervencije: "));
                cell.Colspan = 2;
                cell.Border = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(intervention.From.ToString("d.M.yy")));
                cell.Colspan = 1;
                cell.Border = 0;
                cell.BorderWidthBottom = 1;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(""));
                cell.Colspan = 1;
                cell.Border = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase("Podpis: "));
                cell.Colspan = 2;
                cell.Border = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(""));
                cell.Colspan = 2;
                cell.Border = 0;
                cell.BorderWidthBottom = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Vodja Službe transporta: "));
                cell.Colspan = 3;
                cell.Border = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(""));
                cell.Colspan = 1;
                cell.Border = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(""));
                cell.Colspan = 4;
                cell.Border = 0;
                cell.BorderWidthBottom = 1;
                table.AddCell(cell);

                pdfDoc.Add(table);
            }
        }

        private static int[] GetTakenHours(IEnumerable<Intervention> interventionListSeperated, Duty duty)
        {
            int[] takenHoursInOneDay = new int[8];
            Array.Clear(takenHoursInOneDay, 0, takenHoursInOneDay.Length);

            int[] hoursTaken = new int[24];
            for (int indexDays = 0; indexDays < 8; indexDays++)
            {
                Array.Clear(hoursTaken, 0, hoursTaken.Length);
                foreach (Intervention intervention in interventionListSeperated)
                {
                    if (intervention.From.Day == duty.From.AddDays(indexDays).Day)
                    {
                        int endHour = intervention.From.Day != intervention.To.Day ? 24 : intervention.To.Hour;
                        for (int hour = intervention.From.Hour; hour < endHour; hour++)
                        {
                            hoursTaken[hour]++;
                        }
                    }
                }

                for (int indexHours = 0; indexHours < 24; indexHours++)
                {
                    if (hoursTaken[indexHours] > 0)
                    {
                        takenHoursInOneDay[indexDays]++;
                    }
                }
            }

            return takenHoursInOneDay;
        }

        private static IList<Intervention> SeperateInterventions(IEnumerable<Intervention> interventionList, Duty duty)
        {
            IList<Intervention> interventionListSeperated = new List<Intervention>();

            for (int indexDays = 0; indexDays < 8; indexDays++) //split interval if within work hours
            {
                foreach (Intervention intervention in interventionList)
                {
                    if (intervention.From.Day == duty.From.AddDays(indexDays).Day && indexDays < 5)
                    {
                        int endHour = intervention.From.Day != intervention.To.Day ? 24 : intervention.To.Hour;
                        if (intervention.From.Hour < 7)
                        {
                            Intervention interventionCopy = intervention.GetCopy();
                            interventionCopy.To = new DateTime(interventionCopy.From.Year, interventionCopy.From.Month, interventionCopy.From.Day,
                                endHour > 7 ? 7 : endHour, interventionCopy.From.Minute, interventionCopy.From.Second);
                            interventionListSeperated.Add(interventionCopy);
                        }

                        if (endHour > 15)
                        {
                            Intervention interventionCopy = intervention.GetCopy();
                            interventionCopy.From = new DateTime(interventionCopy.From.Year, interventionCopy.From.Month, interventionCopy.From.Day,
                                interventionCopy.From.Hour < 15 ? 15 : interventionCopy.From.Hour, interventionCopy.From.Minute, interventionCopy.From.Second);
                            interventionListSeperated.Add(interventionCopy);
                        }
                    }
                    if (intervention.From.Day == duty.From.AddDays(indexDays).Day && indexDays >= 5)
                    {
                        interventionListSeperated.Add(intervention);
                    }
                }
            }

            return interventionListSeperated;
        }

        private static void AddHeader(Document pdfDoc, Employee employee, Duty duty)
        {
            pdfDoc.Add(new Paragraph());
            Chunk chunk = new Chunk("SLUŽBA:	Služba transporta\n", FontFactory.GetFont("TIMES_ROMAN", 10, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk);
            chunk = new Chunk("Oddelek: PSO\n", FontFactory.GetFont("TIMES_ROMAN", 10, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk);
            chunk = new Chunk("Delavec: " + employee.Name + " " + employee.Surname + "\n", FontFactory.GetFont("Arial", 10, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk);
            pdfDoc.Add(Chunk.NEWLINE);
            pdfDoc.Add(Chunk.NEWLINE);
            PdfPTable pt = new PdfPTable(1);
            AddCellCenterBold(pt, "OBRAČUN PRIPRAVLJENOSTI IN INTERVENCIJ", 12);
            AddCellCenterBold(pt, ("od " + PlinovodiDezurstvaUtils.SubStringMonthInDate(duty.From.ToString("d.MMMM.yy")) + " do " +
                PlinovodiDezurstvaUtils.SubStringMonthInDate(duty.To.ToString("d.MMMM.yy"))), 12);
            pdfDoc.Add(pt);
        }

        private static void AddFooter(Document pdfDoc, Employee employee, Duty duty, double realHoursAllSum, int[] takenHoursInOneDay)
        {
            PdfPTable pt = new PdfPTable(1);
            AddCellRight(pt, "Skupaj - redne ure " + realHoursAllSum);
            AddCellRight(pt, "Skupaj - interventne ure " + takenHoursInOneDay.Sum()); //interventnih ur je toliko kot je bilo odvzetih

            pdfDoc.Add(pt);

            Chunk chunk = new Chunk("Ljubljana: " + DateTime.Now.ToString("d.M.yyyy") + "\n", FontFactory.GetFont("TIMES_ROMAN", 10, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk);
            chunk = new Chunk("Sestavil " + employee.Name + " " + employee.Surname + "\n", FontFactory.GetFont("TIMES_ROMAN", 10, Font.NORMAL, BaseColor.BLACK));
            pdfDoc.Add(chunk);

            pt = new PdfPTable(1);
            AddCellRight(pt, "Vodja Službe transporta ");
            AddCellRight(pt, " ");
            AddCellRight(pt, " ");
            AddCellRight(pt, "----------------------");

            pdfDoc.Add(pt);
        }

        private static void AddCellRight(PdfPTable pt, string cellText)
        {
            PdfPCell cell = new PdfPCell(new Phrase(cellText));
            cell.VerticalAlignment = Element.ALIGN_RIGHT;
            cell.HorizontalAlignment = Element.ALIGN_RIGHT;
            cell.Border = 0;
            pt.AddCell(cell);
        }

        private static void AddCellCenterBold(PdfPTable pt, string cellText, int fontSize)
        {
            BaseFont bf = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1250, false);
            PdfPCell cell = new PdfPCell(new Phrase(cellText, new Font(bf, fontSize, Font.BOLD)));
            cell.VerticalAlignment = Element.ALIGN_CENTER;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Border = 0;
            pt.AddCell(cell);
        }

        private static PdfPTable GetTable(int size)
        {
            PdfPTable table = new PdfPTable(size);
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 10f;
            table.SpacingAfter = 30f;

            return table;
        }

        private static void AddTableLine(PdfPTable table, IList<string> Cells)
        {
            foreach(string cellText in Cells)
            {
                BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, false);
                PdfPCell cell = new PdfPCell(new Phrase(cellText, new Font(bf)));
                table.AddCell(cell);
            }
        }

        public static int GetSteviloUr(int dayIndex, int[] takenHoursInOneDay)
        {
            int dayHourCount = 0;
            switch (dayIndex)
            {
                case 0:
                    dayHourCount = 9 - takenHoursInOneDay[dayIndex];
                    break;
                case 1:
                    dayHourCount = 16 - takenHoursInOneDay[dayIndex];
                    break;
                case 2:
                    dayHourCount = 16 - takenHoursInOneDay[dayIndex];
                    break;
                case 3:
                    dayHourCount = 16 - takenHoursInOneDay[dayIndex];
                    break;
                case 4:
                    dayHourCount = 16 - takenHoursInOneDay[dayIndex];
                    break;
                case 5:
                    dayHourCount = 24 - takenHoursInOneDay[dayIndex];
                    break;
                case 6:
                    dayHourCount = 24 - takenHoursInOneDay[dayIndex];
                    break;
                case 7:
                    dayHourCount = 7 - takenHoursInOneDay[dayIndex];
                    break;
            }

            return dayHourCount;
        }

        private static string GetDayName(int dayIndex)
        {
            string dayName = "";
            switch(dayIndex)
            {
                case 0:
                    dayName = "PONEDELJEK";
                    break;
                case 1:
                    dayName = "TOREK";
                    break;
                case 2:
                    dayName = "SREDA";
                    break;
                case 3:
                    dayName = "ČETRTEK";
                    break;
                case 4:
                    dayName = "PETEK";
                    break;
                case 5:
                    dayName = "SOBOTA";
                    break;
                case 6:
                    dayName = "NEDELJA";
                    break;
                case 7:
                    dayName = "PONEDELJEK";
                    break;
            }

            return dayName;
        }

        private static string SubStringMonthInDate(string date)
        {
            string[] dateParts = date.Split('.');
            dateParts[1] = dateParts[1].Substring(0, 3);

            return dateParts[0] + "." + dateParts[1] + "." + dateParts[2];
        }
    }
}
