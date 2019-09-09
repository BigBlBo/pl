using System.Collections.Generic;

namespace PlinovodiDezurstva.Models
{
    public class Intervention
    {
        public int Id { get; set; }
        public List<InterventionDay> Days { get; set; }
        public List<InterventionTime> StartTimes { get; set; }
        public List<InterventionDay> EndTimes  { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
    }

    public class InterventionDay
    {
        public int Id { get; set; }
        public string Day { get; set; }
    }

    public class InterventionTime
    {
        public int Id { get; set; }
        public string Time { get; set; }
    }
}