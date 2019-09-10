using PlinovodiDezurstva.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PlinovodiDezurstva.Data
{
    public interface IPlinovodiDutyDataRead
    {
        Task<IEnumerable<Employee>> GetEmployee();
        Task<IEnumerable<Duty>> GetEmployeeDuty(int Id);
        Task<Duty> GetDuty(int Id);
        Task<IEnumerable<Intervention>> GetInterventions(int DutyId);
        Task<Intervention> GetIntervention(int Id);
    }
}
