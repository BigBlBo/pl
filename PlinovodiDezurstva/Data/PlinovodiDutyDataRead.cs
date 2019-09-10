using Dapper;
using Microsoft.Extensions.Options;
using PlinovodiDezurstva.Infrastructure;
using PlinovodiDezurstva.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace PlinovodiDezurstva.Data
{
    public class PlinovodiDutyDataRead : IPlinovodiDutyDataRead
    {
        protected readonly IOptions<ConnectionStringBundle> _options;

        public PlinovodiDutyDataRead(IOptions<ConnectionStringBundle> options)
        {
            this._options = options;
        }

        public async Task<IEnumerable<Employee>> GetEmployee()
        {
            try
            {
                using (var conn = new SqlConnection(_options.Value.ConnectionString))
                {
                    string queryString = @"SELECT ID, NAME, SURNAME FROM  [plinovodiduty].[employee]";

                    IEnumerable<Employee> employeeList = await conn.QueryAsync<Employee>(queryString).ConfigureAwait(false);

                    return employeeList;
                }
            }catch(Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<Duty>> GetEmployeeDuties(int Id)
        {
            try
            {
                using (var conn = new SqlConnection(_options.Value.ConnectionString))
                {
                    DynamicParameters prms = new DynamicParameters();
                    prms.Add("EmployeeId", Id);

                    string queryString = @"SELECT A.ID, A.[FROM], A.[TO] FROM  [plinovodiduty].[Duty] A INNER JOIN
                                                    [plinovodiduty].[employeeonduty] B ON A.Id = B.DutyId WHERE B.EmployeeId = @EmployeeId 
                                                        ORDER BY A.[FROM] ASC";

                    IEnumerable<Duty> dutyList = await conn.QueryAsync<Duty>(queryString, prms).ConfigureAwait(false);

                    return dutyList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Duty> GetDuty(int Id)
        {
            try
            {
                using (var conn = new SqlConnection(_options.Value.ConnectionString))
                {
                    DynamicParameters prms = new DynamicParameters();
                    prms.Add("Id", Id);

                    string queryString = @"SELECT A.ID, A.[FROM], A.[TO] FROM [plinovodiduty].[Duty] A WHERE Id = @Id";

                    IEnumerable<Duty> dutyList = await conn.QueryAsync<Duty>(queryString, prms).ConfigureAwait(false);

                    return dutyList.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<Intervention>> GetInterventions(int DutyId)
        {
            try
            {
                using (var conn = new SqlConnection(_options.Value.ConnectionString))
                {
                    DynamicParameters prms = new DynamicParameters();
                    prms.Add("DutyId", DutyId);

                    string queryString = @"SELECT ID, [FROM], [TO] ,[ShortDescription], [LongDescription] FROM [plinovodiduty].[Intervention] WHERE DutyId = @DutyId";

                    IEnumerable<Intervention> interventionList = await conn.QueryAsync<Intervention>(queryString, prms).ConfigureAwait(false);

                    return interventionList;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Intervention> GetIntervention(int Id)
        {
            try
            {
                 using (var conn = new SqlConnection(_options.Value.ConnectionString))
                {
                    DynamicParameters prms = new DynamicParameters();
                    prms.Add("Id", Id);

                    string queryString = @"SELECT ID, [FROM], [TO] ,[ShortDescription], [LongDescription] FROM [plinovodiduty].[Intervention] WHERE Id = @Id";

                    IEnumerable<Intervention> interventionList = await conn.QueryAsync<Intervention>(queryString, prms).ConfigureAwait(false);

                    return interventionList.FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}