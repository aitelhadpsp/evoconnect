namespace EvoConnect.Server.DTOs;
    public class VipPatientDto
    {
        public int PatientId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public double TotalRevenue { get; set; }
        public double AnnualRevenue { get; set; }

        public DateTime? LastAppointmentDate { get; set; }
        public int? DaysSinceLastVisit { get; set; }
        public int? MonthsSinceLastVisit { get; set; }
        public string LastVisitDisplay { get; set; }

        public int VisitFrequency { get; set; }
        public string FrequencyDisplay => $"{VisitFrequency} visites/an";

        public bool IsAtRisk { get; set; }
        public bool IsActive { get; set; }
        public string Status { get; set; }

        // Helper properties for UI
        public string TotalRevenueFormatted => $"{TotalRevenue:N0} MAD";
        public string AnnualRevenueFormatted => $"{AnnualRevenue:N0} MAD";

        // Priority score for sorting
        public int PriorityScore
        {
            get
            {
                if (!IsAtRisk) return 0;

                int score = 0;

                // Higher revenue = higher priority
                if (TotalRevenue >= 10000) score += 50;
                else if (TotalRevenue >= 5000) score += 30;
                else score += 10;

                // Longer time since visit = higher priority
                if (MonthsSinceLastVisit.HasValue)
                {
                    if (MonthsSinceLastVisit.Value >= 12) score += 40;
                    else if (MonthsSinceLastVisit.Value >= 6) score += 25;
                    else score += 10;
                }

                // Higher frequency in the past = higher priority
                if (VisitFrequency >= 6) score += 30;
                else if (VisitFrequency >= 3) score += 15;

                return score;
            }
        }
    }
public class VipPatientRawDto
{
    public int PatientId { get; set; }
    public string LastName { get; set; }
    public string FirstName { get; set; }
    public string Phone { get; set; }
    public string Email { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AnnualRevenue { get; set; }
    public DateTime? LastAppointmentDate { get; set; }
    public int VisitFrequency { get; set; }
    public int? DaysSinceLastVisit { get; set; }
    public bool IsAtRisk { get; set; }
    public bool IsActive { get; set; }
}