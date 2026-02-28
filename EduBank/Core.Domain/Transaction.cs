using Common;

namespace Core.Domain
{
    public class Transaction : BaseEntity
    {
        public Guid? SourceId { get; set; }
        public TransactionObjectType SourceType { get; set; }

        public Guid? TargetId { get; set; }
        public TransactionObjectType TargetType { get; set; }

        public decimal Amount { get; set; }
        public string Description { get; set; } = null!;

        public TransactionStatus Status { get; set; } // Pending, Completed, Cancelled
        public string? ResolutionMessage { get; set; }

        public DateTime? ResolvedAt { get; set; }
    }
}
