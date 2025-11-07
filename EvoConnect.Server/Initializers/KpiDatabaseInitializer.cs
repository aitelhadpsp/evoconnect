using System;
using System.IO;
using System.Threading.Tasks;
using FirebirdSql.Data.FirebirdClient;

namespace EvoConnect.Server.Initializers
{
    public class KpiDatabaseInitializer
    {
        private readonly string _connectionString;

        public KpiDatabaseInitializer(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Initialize KPI database structure on application startup.
        /// Safe to run multiple times - only creates if not exists.
        /// </summary>
        public async Task InitializeAsync()
        {

            var scripts = new List<string>
            {
                "create_kpi.sql",
                "create_indexes.sql",
                "create_vip_state.sql",
                "SP_REFRESH_VIP_STATS.sql",

            };
            try
            {
                using var connection = new FbConnection(_connectionString);
                await connection.OpenAsync();
                foreach (var scriptFile in scripts)
                { try
                {
                     string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sql", scriptFile);
                    if (File.Exists(scriptPath))
                    {
                        string sqlScript = File.ReadAllText(scriptPath);
                        using var command = new FbCommand(sqlScript, connection);
                        command.CommandTimeout = 120; 
                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch (System.Exception ex)
                {
                    Console.WriteLine($"Error executing script: {scriptFile}",ex);
                    throw;
                }
                   
                }
                
                Console.WriteLine("KPI database initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing KPI database: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Synchronous version for compatibility
        /// </summary>
        public void Initialize()
        {
            InitializeAsync().GetAwaiter().GetResult();
        }
    }
}