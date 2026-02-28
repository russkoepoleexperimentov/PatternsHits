using Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditApplication.Dtos
{
    public class CreditDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid TariffId { get; set; }
        public decimal Amount { get; set; }
        public decimal RemainingDebt { get; set; }
        public int TermDays { get; set; }
        public CreditStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? ApprovedBy { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public string? RejectionReason { get; set; }
    }
}
