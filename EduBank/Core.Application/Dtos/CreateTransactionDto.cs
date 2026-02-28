
namespace Core.Application.Dtos
{
    public class CreateTransactionDto
    {
        public Guid AccountId { get; set; }
        public string Type { get; set; } = null!; // "deposit" | "withdraw"
        public decimal Amount { get; set; }
        public string Description { get; set; } = null!;
    }
}
