using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditApplication.Dtos
{
    public class TariffDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal InterestRate { get; set; }
        public decimal MaxAmount { get; set; }
        public int MaxTermDays { get; set; }
    }
}
