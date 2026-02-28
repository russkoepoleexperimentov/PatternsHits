using Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditDomain.Entities
{
    public class Tariff : BaseEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal InterestRate { get; set; }
        public decimal MaxAmount { get; set; }
        public int MaxTermDays { get; set; }
        public virtual ICollection<Credit> Credits { get; set; } = new List<Credit>();
    }
}