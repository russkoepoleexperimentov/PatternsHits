namespace Dtos
{
    public class OptionsDto
    {
        public string WebTheme { get; set; } = "Light";
        public string MobileTheme { get; set; } = "Red";
        public List<Guid> HiddenAccounts { get; set; } = new();
    }
}
