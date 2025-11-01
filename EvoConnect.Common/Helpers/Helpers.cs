using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvoConnect;
using EvoConnect.Common.Models;
using Newtonsoft.Json;

namespace EvoConnect.Common.Helpers
{
    public class Helpers
    {


        public static string ColorToHex(Color color)
        {
            return $"{color.R:X2}{color.G:X2}{color.B:X2}";
        }
        public static async Task<dynamic> SendData(string serverPath, dynamic data)
        {

            using HttpClient httpClient = new();
            var token = AppData.GetApiKey();

            var payload = new
            {
                Token = token,
                Data = data
            };
            dynamic content;
            if (data is not MultipartFormDataContent)
            {
                string jsonData = JsonConvert.SerializeObject(payload);
                content = new StringContent(jsonData, Encoding.UTF8, "application/json");
            }
            else
            {
                data.Add(new StringContent(token), "Token");
                content = data;
            }
            HttpResponseMessage response = await httpClient.PostAsync(serverPath, content);
            response.EnsureSuccessStatusCode();
            httpClient.Dispose();
            return await response.Content.ReadAsStringAsync();
        }
        public static async Task LogErrorToFileAsync(Exception ex)
        {
            try
            {
                var logFilePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
                    "dev_connect",
                    "errors.log"
                );

                var errorMessage = $"Timestamp: {DateTime.UtcNow}\n" +
                                  $"Message: {ex.Message}\n" +
                                  $"StackTrace: {ex.StackTrace}\n" +
                                  $"------------------------------\n";

                var logDirectory = Path.GetDirectoryName(logFilePath);

                // Ensure directory exists
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // Check if file exists and its size
                if (File.Exists(logFilePath))
                {
                    var fileInfo = new FileInfo(logFilePath);
                    if (fileInfo.Length > 1024 * 1024 * 200) 
                    {
                        // If file is too large, overwrite it
                        await File.WriteAllTextAsync(logFilePath, errorMessage);
                    }
                    else
                    {
                        // Append to existing file
                        await File.AppendAllTextAsync(logFilePath, errorMessage);
                    }
                }
                else
                {
                    // Create new file
                    await File.WriteAllTextAsync(logFilePath, errorMessage);
                }
            }
            catch (Exception logEx)
            {
                // Handle any errors during logging
                Console.WriteLine($"Error writing to log file: {logEx.Message}");
            }
        }
    }
}