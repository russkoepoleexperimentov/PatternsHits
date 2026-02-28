using Common;
using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditDomain.Entities
{
    public class Payment : BaseEntity
    {
        public Guid CreditId { get; set; }
        public Credit Credit { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string FailureReason { get; set; } = null!;
    }
}
