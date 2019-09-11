using iTextSharp.text;
using iTextSharp.text.pdf;
using PlinovodiDezurstva.Models;
using System;
using System.Collections.Generic;
using System.IO;

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
            int realHoursAllSumIntervention = 0;
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
                PlinovodiDezurstvaUtils.AddTableLine(table, new List<string>() { PlinovodiDezurstvaUtils.SubStringMonthInDate(intervention.From.ToString("d.MMMM.yy")), "",
                        intervention.From.ToString("HH:mm"), intervention.To.ToString("HH:mm"), intervention.ShortDescription });

                realHoursAllSumIntervention += intervention.To.Hour - intervention.From.Hour;
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
            PlinovodiDezurstvaUtils.AddFooter(pdfDoc, employee, duty, realHoursAllSum, realHoursAllSumIntervention);
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
                cell.Colspan = 1;
                cell.Border = 0;
                table.AddCell(cell);

                //row4
                cell = new PdfPCell(new Phrase("Zaključek ob:: "));
                cell.Colspan = 2;
                cell.Border = 0;
                table.AddCell(cell);
                cell = new PdfPCell(new Phrase(intervention.To.ToString("HH:mm")));
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
                cell = new PdfPCell(new Phrase((intervention.To.Hour - intervention.From.Hour).ToString()));
                cell.Colspan = 2;
                cell.Border = 0;
                cell.BorderWidthBottom = 1;
                table.AddCell(cell);



                pdfDoc.Add(table);

                Chunk chunk = new Chunk("Kratek opis javljenega nedovoljenega stanja:\n", FontFactory.GetFont(BaseFont.TIMES_ROMAN, 14, Font.BOLD, BaseColor.BLACK));
                pdfDoc.Add(chunk);
                chunk = new Chunk("Stanje ob prihodu:\n", FontFactory.GetFont(BaseFont.TIMES_ROMAN, 14, Font.BOLD, BaseColor.BLACK));
                pdfDoc.Add(chunk);
                chunk = new Chunk("Ukrepi in opis opravljenega dela:\n", FontFactory.GetFont(BaseFont.TIMES_ROMAN, 14, Font.BOLD, BaseColor.BLACK));
                pdfDoc.Add(chunk);
                chunk = new Chunk("Vzrok:\n", FontFactory.GetFont(BaseFont.TIMES_ROMAN, 14, Font.BOLD, BaseColor.BLACK));
                pdfDoc.Add(chunk);
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
                        for (int hour = intervention.From.Hour; hour < intervention.To.Hour; hour++)
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
                        if (intervention.From.Hour < 7)
                        {
                            Intervention interventionCopy = intervention.GetCopy();
                            interventionCopy.To = new DateTime(interventionCopy.To.Year, interventionCopy.To.Month, interventionCopy.To.Day,
                                interventionCopy.To.Hour > 7 ? 7 : interventionCopy.To.Hour, interventionCopy.To.Minute, interventionCopy.To.Second);
                            interventionListSeperated.Add(interventionCopy);
                        }

                        if (intervention.To.Hour > 15)
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

        private static void AddFooter(Document pdfDoc, Employee employee, Duty duty, double realHoursAllSum, int realHoursAllSumIntervention)
        {
            PdfPTable pt = new PdfPTable(1);
            AddCellRight(pt, "Skupaj - redne ure " + realHoursAllSum);
            AddCellRight(pt, "Skupaj - interventne ure " + realHoursAllSumIntervention);

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
            table.SpacingBefore = 20f;
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
                    dayHourCount = 10 - takenHoursInOneDay[0];
                    break;
                case 1:
                    dayHourCount = 16 - takenHoursInOneDay[1];
                    break;
                case 2:
                    dayHourCount = 16 - takenHoursInOneDay[2];
                    break;
                case 3:
                    dayHourCount = 16 - takenHoursInOneDay[2];
                    break;
                case 4:
                    dayHourCount = 16 - takenHoursInOneDay[4];
                    break;
                case 5:
                    dayHourCount = 24 - takenHoursInOneDay[5];
                    break;
                case 6:
                    dayHourCount = 24 - takenHoursInOneDay[6];
                    break;
                case 7:
                    dayHourCount = 6 - takenHoursInOneDay[7];
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
