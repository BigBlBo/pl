﻿using System;
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
    }
}