using Common;

namespace Core.Domain
{
    // счет в банке
    public class Account : BaseEntity
    {
        public Guid UserId { get; set; }
        public decimal Balance { get; set; }
        public DateTime? ClosedAt { get; set; }

        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
