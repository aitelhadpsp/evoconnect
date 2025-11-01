/* using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EvoConnect.Server.Models;

namespace evodental.Infrastructure.Entity
{
    public class CnssSubmission 
    {
        public string Reference { get; set; } = string.Empty;
        public Utilisateur User { get; set; }
        public Guid DoctorId { get; set; }
        public Guid PatientId { get; set; }
        public Patient Patient { get; set; }
        public double Amount { get; set; }
        public CnssSubmissionStatus Status { get; set; } = CnssSubmissionStatus.Pending;
        public List<CnssSubmissionItem> Items { get; set; }
        public List<CnssSubmissionFile> Files { get; set; }
        public List<CnssSubmissionPrescription> Prescriptions { get; set; }
    }

    public enum CnssSubmissionStatus
    {
        Pending,
        Accepted,
        Rejected,
    }
}
 */