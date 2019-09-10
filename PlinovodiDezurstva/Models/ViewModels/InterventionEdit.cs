using System.Collections.Generic;

namespace PlinovodiDezurstva.Models.ViewModels
{
    public class InterventionEdit
    {
        public InterventionEdit()
        {
            StartTimes = new List<InterventionTime>();
            for (int index = 0; index <= 23; index++)
            {
                StartTimes.Add(new InterventionTime { Id = index, Time = index.ToString() + ":00" });
            }

            EndTimes = new List<InterventionTime>();
            for (int index = 1; index <= 24; index++)
            {
                EndTimes.Add(new InterventionTime { Id = index, Time = index.ToString() + ":00" });
            }
        }

        public int Id { get; set; }
        public List<InterventionDay> Days { get; set; } = new List<InterventionDay>();
        public List<InterventionTime> StartTimes { get; set; }
        public List<InterventionTime> EndTimes  { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }

        public int SelectedDay = 0;
        public int SelectedStartTime = 0;
        public int SelectedEndTime = 1;
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