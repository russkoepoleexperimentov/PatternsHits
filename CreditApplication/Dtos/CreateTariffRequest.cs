using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditApplication.Dtos
{
    public class CreateTariffRequest
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [Range(0.01, 100.0)]
        public decimal InterestRate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal MaxAmount { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int MaxTermDays { get; set; }
    }

    public class UpdateTariffRequest : CreateTariffRequest { }
}
