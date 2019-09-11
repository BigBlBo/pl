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
        public static MemoryStream GetPregeledUrReport(Employee employee, Duty duty, int[] takenHoursInOneDay, IEnumerable<Intervention> interventionList)
        {
            var stream = new MemoryStream();
            Document pdfDoc = new Document(PageSize.A4, 25, 25, 25, 15);
            PdfWriter pdfWriter = PdfWriter.GetInstance(pdfDoc, stream);
            pdfDoc.Open();

            PlinovodiDezurstvaUtils.AddHeader(pdfDoc, employee, duty);

            PdfPTable table = PlinovodiDezurstvaUtils.GetTable();

            PlinovodiDezurstvaUtils.AddTableLine(table, new List<string>() { "Datum", "Dan", "", "Število ur", "Število rednih  ur (10%)" });

            double realHoursAllSum = 0;
            int realHoursAllSumIntervention = 0;
            int hoursSum = 0;
            double realHoursSum = 0;
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


            table = PlinovodiDezurstvaUtils.GetTable();

            PlinovodiDezurstvaUtils.AddTableLine(table, new List<string>() { "Datum", "Dan", "", "Število ur", "Število rednih  ur (20%)" });

            hoursSum = 0;
            realHoursSum = 0;
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

            PdfPTable pt = new PdfPTable(1);
            PlinovodiDezurstvaUtils.AddCellCenterBold(pt, "INTERVENCIJA");
            pdfDoc.Add(pt);

            table = PlinovodiDezurstvaUtils.GetTable();

            PlinovodiDezurstvaUtils.AddTableLine(table, new List<string>() { "Datum", "Objekt", "Od ure", "Do ure", "Opis del" });

            foreach (Intervention intervention in interventionList)
            {
                PlinovodiDezurstvaUtils.AddTableLine(table, new List<string>() { PlinovodiDezurstvaUtils.SubStringMonthInDate(intervention.From.ToString("d.MMMM.yy")), "",
                        intervention.From.ToString("HH:mm"), intervention.To.ToString("HH:mm"), intervention.ShortDescription });

                realHoursAllSumIntervention += intervention.To.Hour - intervention.From.Hour;
            }
            pdfDoc.Add(table);

            PlinovodiDezurstvaUtils.AddFooter(pdfDoc, employee, duty, realHoursAllSum, realHoursAllSumIntervention);

            pdfWriter.CloseStream = false;
            pdfDoc.Close();

            return stream;
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
            AddCellCenterBold(pt, "OBRAČUN PRIPRAVLJENOSTI IN INTERVENCIJ");
            AddCellCenterBold(pt, ("od " + PlinovodiDezurstvaUtils.SubStringMonthInDate(duty.From.ToString("d.MMMM.yy")) + " do " +
                PlinovodiDezurstvaUtils.SubStringMonthInDate(duty.To.ToString("d.MMMM.yy"))));
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

        private static void AddCellCenterBold(PdfPTable pt, string cellText)
        {
            PdfPCell cell = new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.TIMES_ROMAN, 12, Font.BOLD)));
            cell.VerticalAlignment = Element.ALIGN_CENTER;
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
            cell.Border = 0;
            pt.AddCell(cell);
        }

        private static PdfPTable GetTable()
        {
            PdfPTable table = new PdfPTable(5);
            table.WidthPercentage = 100;
            table.HorizontalAlignment = 0;
            table.SpacingBefore = 20f;
            table.SpacingAfter = 30f;

            return table;
        }

        private static void AddTableLine(PdfPTable table, IList<string> Cells)
        {
            foreach(string cell in Cells)
            {
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
                    dayName = "CETRTEK";
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
