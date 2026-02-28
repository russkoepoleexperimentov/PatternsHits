
namespace Core.Application.Dtos
{
    public class CreateAccountDto
    {
        public Guid UserId { get; set; }
        public string Currency { get; set; } = null!;
        public decimal InitialBalance { get; set; }
    }
}
