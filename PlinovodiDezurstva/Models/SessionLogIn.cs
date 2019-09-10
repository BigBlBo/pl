using System;
using System.Collections.Generic;

namespace PlinovodiDezurstva.Models
{
    public class SessionLogIn
    {
        public int EmployeeId { get; set; }
        public int DutyId { get; set; }
        public IList<DateTime> DaysOfDuty { get; set; } = new List<DateTime>();
    }
}