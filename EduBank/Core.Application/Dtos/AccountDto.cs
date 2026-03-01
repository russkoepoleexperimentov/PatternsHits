
namespace Core.Application.Dtos
{
    public class AccountDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public decimal Balance { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? ClosedAt { get; set; }
    }
}
