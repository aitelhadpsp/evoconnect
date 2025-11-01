using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Server.Services.cnss
{
    public class AuthResponse
    {
        public bool Code { get; set; }
        public string message { get; set; }
        public string token { get; set; }
        public string refreshToken { get; set; }
    }
}
