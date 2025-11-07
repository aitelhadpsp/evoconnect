namespace EvoConnect.Common
{
    public class AppConfigs
    {
        public string ConnectionString { get; set; } = "";
        public int Port { get; set; } = 6236;
        public bool EnableMdns { get; set; } = true;
    }

}