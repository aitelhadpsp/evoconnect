using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EvoConnect.Server.Services.cnss
{
    public class DeclarationResponseDto
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("numeroFse")]
        public string? NumeroFse { get; set; }

        //[JsonPropertyName("prestations")]
        // public List<PrestationDto>? Prestations { get; set; }
    }

    public class PrestationDto
    {
        public int Id { get; set; }
        public string Libelle { get; set; }
        public int Nombre { get; set; }
        public double PrixUnitaire { get; set; }
        public DateTime DateRealisation { get; set; }
        public DateTime DateSeancePrescription { get; set; }
    }
}
