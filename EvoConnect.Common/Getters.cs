using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EvoConnect.Common.Models;
using Newtonsoft.Json;

namespace EvoConnect.Common
{

    public class Getters
    {


        public static async Task<Partner?> GetPartner()
        {
            var token = AppData.GetApiKey();
            var serverPath = $"{Api.GetByToken}{token}";
            using HttpClient client = new();
            try
            {
                HttpResponseMessage response = await client.GetAsync(serverPath);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var res = JsonConvert.DeserializeObject<Partner>(responseBody);
                    DbContext appDbContext = new();
                    appDbContext.UpdateFlashEvo(res?.FlashEvo ?? false);
                    return res;
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine($"Exception occurred: {ex.Message}");
            }

            return null;
        }
        public static async Task<Partner?> GetPartnerByCode(string key)
        {

            var serverPath = $"{Api.GetByToken}{key}";
            using HttpClient client = new();
            HttpResponseMessage response = await client.GetAsync(serverPath);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            var res = JsonConvert.DeserializeObject<Partner>(responseBody);
            DbContext appDbContext = new();
            appDbContext.UpdateFlashEvo(res?.FlashEvo ?? false);
            return res;



        }
    }
}