
using Core.Domain;

namespace Core.Application.Dtos
{
    public class TransactionDto
    {
        public Guid Id { get; set; }

        public Guid? SourceId { get; set; }
        public TransactionObjectType SourceType { get; set; }

        public Guid? TargetId { get; set; }
        public TransactionObjectType TargetType { get; set; }

        public decimal Amount { get; set; }
        public string Description { get; set; } = null!;
        public TransactionStatus Status { get; set; } // Pending, Completed, Cancelled
        public string? ResolutionMessage { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
