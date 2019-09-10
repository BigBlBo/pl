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
    public class PlinovodiDutyDataWrite : IPlinovodiDutyDataWrite
    {
        protected readonly IOptions<ConnectionStringBundle> _options;

        public PlinovodiDutyDataWrite(IOptions<ConnectionStringBundle> options)
        {
            this._options = options;
        }

        public async Task InsertIntervention(Intervention Intervention)
        {
            using (var conn = new SqlConnection(_options.Value.ConnectionString))
            {
                DynamicParameters prms = new DynamicParameters();
                prms.Add("DutyId", Intervention.DutyId);
                prms.Add("From", Intervention.From);
                prms.Add("To", Intervention.To);
                prms.Add("ShortDescription", Intervention.ShortDescription);
                prms.Add("LongDescription", Intervention.LongDescription);

                string queryString = @"INSERT INTO [plinovodiduty].[intervention]
                                           ([DutyId]
                                           ,[From]
                                           ,[To]
                                           ,[ShortDescription]
                                           ,[LongDescription])
                                     VALUES
                                            (@DutyId, @From, @To, @ShortDescription, @LongDescription)";

                await conn.ExecuteAsync(queryString, prms).ConfigureAwait(false);
            }
        }

        public async Task UpdateIntervention(Intervention Intervention)
        {
            using (var conn = new SqlConnection(_options.Value.ConnectionString))
            {
                DynamicParameters prms = new DynamicParameters();
                prms.Add("Id", Intervention.Id);
                prms.Add("From", Intervention.From);
                prms.Add("To", Intervention.To);
                prms.Add("ShortDescription", Intervention.ShortDescription);
                prms.Add("LongDescription", Intervention.LongDescription);

                string queryString = @"UPDATE [plinovodiduty].[intervention] SET
                                            [From] = @From
                                           ,[To] = @To
                                           ,[ShortDescription] = @ShortDescription
                                           ,[LongDescription] = @LongDescription
                                     WHERE
                                            Id = @Id";

                await conn.ExecuteAsync(queryString, prms).ConfigureAwait(false);
            }
        }
    }
}
