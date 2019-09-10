using PlinovodiDezurstva.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlinovodiDezurstva.Data
{
    public interface IPlinovodiDutyDataWrite
    {
        Task InsertIntervention(Intervention Intervention);
        Task UpdateIntervention(Intervention Intervention);
        Task DeleteIntervention(Intervention Intervention);
    }
}
