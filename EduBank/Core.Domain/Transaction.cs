using Common;

namespace Core.Domain
{
    public class Transaction : BaseEntity
    {
        public Guid FromAccountId { get; set; }
        public TransactionType Type { get; set; } // Deposit / Withdraw /...
        public Guid? TargetId { get; set; } // for transfer / credit 
        public decimal Amount { get; set; }
        public string Description { get; set; } = null!;
        public TransactionStatus Status { get; set; } // Pending, Completed, Cancelled
        public string? ResolutionMessage { get; set; }
        public DateTime? CompletedAt { get; set; }

        public virtual Account FromAccount { get; set; } = null!;
    }
}
