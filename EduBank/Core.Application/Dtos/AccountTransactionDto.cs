using Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Dtos
{
    public class AccountTransactionDto
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!; 
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public TransactionStatus Status { get; set; }
        public string? ResolutionMessage { get; set; }
    }
}
