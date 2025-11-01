using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EvoConnect.Common.Models
{
    public class Partner
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool Active { get; set; }
        public bool Sending { get; set; }
        public bool SmileEvo { get; set; }
        public bool FlashEvo { get; set; }
        public string OwnerId { get; set; }
        public string? PhoneNumber { get; set; }
        public string MessagingProviderId { get; set; }
        public decimal Balance { get; set; }
        public DateTime? LastSync { get; set; }
        public bool CanSendSms { get; set; }
        public bool CanSendWhatsapp { get; set; }
        public int SubscriptionType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}