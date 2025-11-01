using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Server.Models
{
    public class UserCnssData
    {
        public int Id { get; set; }
        public Utilisateur User { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
