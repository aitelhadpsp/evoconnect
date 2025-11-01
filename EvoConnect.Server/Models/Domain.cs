namespace EvoConnect.Server.Models
{

    public class T_CHAINE30
    {
        public static implicit operator string(T_CHAINE30 value) => value?.Value;
        public static implicit operator T_CHAINE30(string value) => new T_CHAINE30 { Value = value };
        public string Value { get; set; }
    }

    public class T_TELNUM
    {
        public static implicit operator string(T_TELNUM value) => value?.Value;
        public static implicit operator T_TELNUM(string value) => new T_TELNUM { Value = value };
        public string Value { get; set; }
    }

}