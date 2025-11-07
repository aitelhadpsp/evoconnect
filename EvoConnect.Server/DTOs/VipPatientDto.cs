namespace EvoConnect.Server.DTOs;
      public class VipPatientDto
    {
        public int ID_PERSONNE { get; set; }
        public string NOM { get; set; }
        public string PRENOM { get; set; }
        public decimal CA_ANNUEL { get; set; }
        public decimal CA_GLOBAL { get; set; }
        public DateTime? DERNIERE_VISITE { get; set; }
        public int? MOIS_DEPUIS_DERNIERE_VISITE { get; set; }
        public string STATUT { get; set; }
    }
    public class CountResult
    {
        public int TOTAL_COUNT { get; set; }
    }
