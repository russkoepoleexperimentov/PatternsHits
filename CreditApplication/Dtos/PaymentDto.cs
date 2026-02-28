using Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditApplication.Dtos
{
    public class PaymentDto
    {
        public Guid Id { get; set; }
        public Guid CreditId { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }

    public class CreatePaymentRequest
    {
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }
        public DateTime? PaymentDate { get; set; }
    }

    public class UpdatePaymentStatusRequest
    {
        [Required]
        public PaymentStatus Status { get; set; }
    }
}
