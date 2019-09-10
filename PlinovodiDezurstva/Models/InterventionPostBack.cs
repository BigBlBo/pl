using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
    }
}
