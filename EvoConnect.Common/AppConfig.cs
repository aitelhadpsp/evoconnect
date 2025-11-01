namespace EvoConnect.Common
{
    public class AppConfigs
    {
        public string ConnectionString { get; set; } = "";
        public int Port { get; set; } = 6222;
        public bool EnableMdns { get; set; } = true;
    }

}