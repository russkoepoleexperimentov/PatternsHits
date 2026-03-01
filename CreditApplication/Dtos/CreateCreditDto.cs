using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditApplication.Dtos
{
    public class CreateCreditRequest
    {
        [Required]
        public Guid AccountId { get; set; }

        [Required]
        public Guid TariffId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int TermDays { get; set; }
    }

    public class ApproveCreditRequest
    {
        public decimal? ApprovedAmount { get; set; }
        public string? Comment { get; set; }
    }

    public class RejectCreditRequest
    {
        public string? Reason { get; set; }
    }

}
