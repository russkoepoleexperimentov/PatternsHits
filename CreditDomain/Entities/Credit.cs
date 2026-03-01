using Common;
using Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditDomain.Entities
{
    public class Credit : BaseEntity
    {

        public Guid UserId { get; set; }
        public Guid AccountId { get; set; }
        public Guid TariffId { get; set; }
        public virtual Tariff Tariff { get; set; }

        public decimal Amount { get; set; }

        public decimal RemainingDebt { get; set; }

        [Required]
        public int TermDays { get; set; }

        public CreditStatus Status { get; set; }

        public Guid? ApprovedBy { get; set; }

        public decimal? ApprovedAmount { get; set; }

        public string RejectionReason { get; set; } = null!;

        public DateTime? ApprovedAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
