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
            try
            {
                using var connection = new FbConnection(_connectionString);
                await connection.OpenAsync();

                string sqlScript = GetInitializationScript();
                
                using var command = new FbCommand(sqlScript, connection);
                command.CommandTimeout = 120; // 2 minutes
                
                await command.ExecuteNonQueryAsync();
                
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

        private string GetInitializationScript()
        {
      
            string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"sql","create_kpi.sql");
            if (File.Exists(scriptPath))
            {
                return File.ReadAllText(scriptPath);
            }

            return @"";
        }
    }
}