using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Common.Models
{
    public class Note
    {
           public int InternId { get; set; }
            public int PatientId { get; set; }
            public byte[]? Done { get; set; }
            public byte[]? ToBeDone { get; set; }
            public DateTime CreatedAt { get; set; }
    }
}