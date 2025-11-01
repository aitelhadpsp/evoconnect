using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Common.Models
{

    public class SyncImageLabel
    {
        public int Id { get; set; }
        public int LabelId { get; set; }

    }
    public class ImageFile
    {
        public int InternId { get; set; }
        public int PatientId { get; set; }
        public int Type { get; set; }
        public int Label { get; set; }
        public dynamic Image { get; set; }
        public List<SyncImageLabel> Labels { get; set; }

    }

    public class ImageDocument {
        public int Id { get; set; }
        public string Path {get;set;}
        public string Name {get;set;}
        public int PatientId {get;set;}
        public DateTime? Date { get; set; }
        public List<int>? Labels { get; set; }
    }
}