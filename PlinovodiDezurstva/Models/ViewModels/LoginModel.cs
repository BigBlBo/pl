using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlinovodiDezurstva.Models.ViewModels
{
    public class LoginModel
    {
        public List<Dezurni> DezurniModel { get; set; } = new List<Dezurni>();
    }
    public class Dezurni
    {
        public int Id { get; set; }
        public string ImePriimek { get; set; }
    }
    public class Interval
    {
        public int Id { get; set; }
        public string Obdobje { get; set; }
    }
}