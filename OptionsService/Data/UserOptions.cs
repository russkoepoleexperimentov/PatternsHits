namespace Data
{
    public class UserOptions
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public WebTheme WebTheme { get; set; } = WebTheme.Light;
        public MobileTheme MobileTheme { get; set; } = MobileTheme.Red;
        public string HiddenAccountsJson { get; set; } = "[]";
        public DateTime LastUpdated { get; set; }
    }
}
