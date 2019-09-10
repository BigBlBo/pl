using Newtonsoft.Json;

namespace PlinovodiDezurstva.Models
{
    public class InterventionPostBack
    {
        public int Id { get; set; }
        public string ShortDescription { get; set; }
        public int InterventionDayId { get; set; }
        public int InterventionTimeStartId { get; set; }
        public int InterventionTimeEndId { get; set; }
        public string LongDescription { get; set; }

        public override string ToString() { return JsonConvert.SerializeObject(this); }
    }
}