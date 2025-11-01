using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoConnect.Common;
using FirebirdSql.Data.FirebirdClient;
using Newtonsoft.Json;

namespace EvoConnect.Server.Sync
{
    public class LocalSyncronise
    {
        private readonly DbContext appDbContext;
        public LocalSyncronise()
        {
            appDbContext = new DbContext();
        }
        public async Task UpdateConfirmation(List<int> ids, List<Guid> guids, string confirmation)
        {
            var fbconnection = new FbConnection(AppData.ConnectionString());
            try
            {
                await fbconnection.OpenAsync();

                var commandText = new StringBuilder("UPDATE RENDEZ_VOUS SET STATUT_CONFIRMATION=@confirmation WHERE ID_RDV IN (");

                var parameters = new List<FbParameter>
            {
                new("@confirmation", FbDbType.VarChar) { Value = confirmation }
            };
                for (int i = 0; i < ids.Count; i++)
                {
                    var paramName = $"@id{i}";
                    commandText.Append($"{paramName},");
                    parameters.Add(new FbParameter(paramName, FbDbType.Integer) { Value = ids[i] });
                }

                commandText.Length--;
                commandText.Append(')');

                FbCommand command = new FbCommand(commandText.ToString(), fbconnection);
                foreach (var param in parameters)
                {
                    command.Parameters.Add(param);
                }
                 await command.ExecuteNonQueryAsync();
                await SendSynchronized(guids);
            }
            catch
            {

            }
            finally
            {
                await fbconnection.CloseAsync();

            }

        }


        public async Task SendSynchronized(List<Guid> ids)
        {
            var token = AppData.GetApiKey();


            var data = new
            {
                Token = token,
                Data = ids
            };

            string serverPath = Api.synchronized;
            HttpClient httpClient = new();

            string jsonData = JsonConvert.SerializeObject(data);
            HttpContent content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            try
            {
                HttpResponseMessage response = await httpClient.PostAsync(serverPath, content);
            }
            catch { }


        }
    }

}