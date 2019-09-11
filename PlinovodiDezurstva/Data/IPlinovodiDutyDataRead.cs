using PlinovodiDezurstva.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PlinovodiDezurstva.Data
{
    public interface IPlinovodiDutyDataRead
    {
        Task<IEnumerable<Employee>> GetEmployees();
        Task<Employee> GetEmployee(int Id);
        Task<IEnumerable<Duty>> GetEmployeeDuties(int Id);
        Task<Duty> GetDuty(int Id);
        Task<IEnumerable<Intervention>> GetInterventions(int DutyId);
        Task<Intervention> GetIntervention(int Id);
    }
}
