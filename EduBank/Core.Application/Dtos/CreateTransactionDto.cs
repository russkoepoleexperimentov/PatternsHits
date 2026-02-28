
using Core.Domain;

namespace Core.Application.Dtos
{
    public class CreateTransactionDto
    {
        public Guid? SourceId { get; set; }
        public TransactionObjectType SourceType { get; set; }

        public Guid? TargetId { get; set; }
        public TransactionObjectType TargetType { get; set; }

        public decimal Amount { get; set; }
        public string Description { get; set; } = null!;
    }
}
