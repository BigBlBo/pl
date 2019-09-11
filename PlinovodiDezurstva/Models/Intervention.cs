using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlinovodiDezurstva.Models
{
    public class Intervention
    {
        public int Id { get; set; }
        public int DutyId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }

        public Intervention GetCopy()
        {
            Intervention intervention = new Intervention();
            intervention.Id = Id;
            intervention.DutyId = DutyId;
            intervention.From = new DateTime(From.Year, From.Month, From.Day, From.Hour, From.Minute, From.Second);
            intervention.To = new DateTime(To.Year, To.Month, To.Day, To.Hour, To.Minute, To.Second);
            intervention.ShortDescription = ShortDescription;
            intervention.LongDescription = LongDescription;

            return intervention;
        }
    }
}