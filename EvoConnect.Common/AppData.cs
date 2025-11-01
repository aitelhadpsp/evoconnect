using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using FirebirdSql.Data.FirebirdClient;

namespace EvoConnect.Common
{
    public class AppData
    {
        public static string? ConnectionString()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\NEURONET\DENTALEVO");
            var basePath = key?.GetValue("BaseDir")?.ToString();
            key?.Close();

            if (basePath is null)
                return null;

            List<string> pathParts = [.. basePath.Split("\\\\")];
            var exist = pathParts.ElementAtOrDefault(1);
            var dataSource = exist is not null ? exist : "localhost";

            var builder = new FbConnectionStringBuilder
            {
                UserID = "SYSDBA",
                Password = "masterkey",
                Database = basePath,
                DataSource = dataSource,
                Port = 3050,
                Dialect = 3,
                Charset = "NONE"
            };

            if (exist is not null)
            {
                builder.ConnectionLifeTime = 0;
                builder.ConnectionTimeout = 7;
                builder.Pooling = true;
                builder.PacketSize = 8192;
                builder.ServerType = 0;
            }

            return builder.ConnectionString;
        }
        public static string? EfConnectionString()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\NEURONET\DENTALEVO");
            var basePath = key?.GetValue("BaseDir")?.ToString();
            key?.Close();

            if (basePath is null)
                return null;

            List<string> pathParts = [.. basePath.Split("\\\\")];
            var exist = pathParts.ElementAtOrDefault(1);
            var dataSource = exist is not null ? exist : "localhost";

            var builder = new FbConnectionStringBuilder
            {
                UserID = "SYSDBA",
                Password = "masterkey",
                Database = basePath,
                DataSource = dataSource,
                Port = 3050,
                Dialect = 3,
                Charset = "NONE"
            };

            if (exist is not null)
            {
                builder.ConnectionLifeTime = 0;
                builder.ConnectionTimeout = 7;
                builder.Pooling = true;
                builder.PacketSize = 8192;
                builder.ServerType = 0;
            }

            return builder.ConnectionString;
        }
        public static bool IsServer()
        {

            return RegGet("TYPE_INSTALL") == "SERVER";


        }
        public static string ImageDir()
        {
            return RegGet("imageriedir") ?? "";
        }
        public static string UploadDir()
        {
            return RegGet("WifiDir")?? "";
        }
        public static void RegSet(string key, string value)
        {
            var reg = GetRegistry(true);

            reg?.SetValue(key, value);
            reg.Close();
        }
        public static RegistryKey GetRegistry(bool setMode = false)
        {
            var users = Registry.Users.GetSubKeyNames();

            var user = users.FirstOrDefault(e =>
              {
                  try
                  {

                      using var key = Registry.Users.OpenSubKey(e)?.OpenSubKey(@"SOFTWARE\NEURONET\DENTALEVO");
                      return key != null;
                  }
                  catch
                  {
                      return false;
                  }

              });
            return Registry.Users.OpenSubKey(user)?.OpenSubKey(@"SOFTWARE\NEURONET\DENTALEVO", setMode);

        }
        public static string? RegGet(string key)
        {

            var reg = GetRegistry();
            var value = reg?.GetValue(key)?.ToString();
            reg?.Close();

            return value;
        }
        public static string? GetApiKey()
        {
            return RegGet("Apikey");
        }
        public static void SetApiKey(string value)
        {
            RegSet("Apikey", value);

        }
    }
}