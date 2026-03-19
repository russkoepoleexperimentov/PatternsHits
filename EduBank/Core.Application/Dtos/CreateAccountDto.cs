namespace Core.Application.Dtos
{
    public class CreateAccountDto
    {
        public decimal InitialBalance { get; set; }
        public string Currency { get; set; } = "RUB";
    }
}
