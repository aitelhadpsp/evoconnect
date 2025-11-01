using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EvoConnect.Common.Models
{
    public class AppointmentCollect
    {
   
            public AppointmentStatusEnum Status { get; set; }
            public List<int> InternIds { get; set; }
            public List<Guid> Ids { get; set; }
     
    }
    public enum AppointmentStatusEnum
    {
        None,
        Sent,
        Delivered,
        Opened,
        Accepted,
        Refused,
        DelayRequested,
        Error
    }
    
    
}
