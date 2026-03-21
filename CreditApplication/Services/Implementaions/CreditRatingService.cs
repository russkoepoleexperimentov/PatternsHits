using CreditInfrastructure;
using Microsoft.EntityFrameworkCore;
using Common.Enums;
using CreditApplication.Services.Interfaces;

namespace CreditService.Services
{
    public class CreditRatingService : ICreditRatingService
    {
        private readonly CreditDbContext _context;

        public CreditRatingService(CreditDbContext context)
        {
            _context = context;
        }

        public async Task<int> GetCreditRatingAsync(Guid userId)
        {
            var credits = await _context.Credits
                .Include(c => c.Payments)
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.CreateDateTime)
                .ToListAsync();

            int rating = 500;
            DateTime now = DateTime.UtcNow;

            var firstCredit = credits.OrderBy(c => c.CreateDateTime).FirstOrDefault();
            if (firstCredit != null)
            {
                var monthsSinceFirst = (now - firstCredit.CreateDateTime).TotalDays / 30;
                rating += (int)Math.Min(monthsSinceFirst, 60);
            }

            int closedCredits = credits.Count(c => c.Status == CreditStatus.Closed);
            for (int i = 0; i < closedCredits; i++)
            {
                rating += Math.Max(50 - i * 10, 10);
            }

            foreach (var credit in credits)
            {
                if (credit.Status == CreditStatus.Approved)
                {
                    rating += 20;

                    if (credit.ApprovedAmount.HasValue && credit.ApprovedAmount > 0)
                    {
                        decimal paidRatio = 1m - (credit.RemainingDebt / credit.ApprovedAmount.Value);
                        rating += (int)(paidRatio * 50);
                    }
                }

                int paymentIndex = 0;
                var payments = credit.Payments
                    .Where(p => p.Status != PaymentStatus.Pending)
                    .OrderBy(p => p.DueDate)
                    .ToList();

                foreach (var payment in payments)
                {
                    double recencyWeight = 1.0 + (paymentIndex / (double)payments.Count);
                    paymentIndex++;

                    if (payment.Status == PaymentStatus.Processed && payment.ProcessedAt.HasValue)
                    {
                        if (payment.ProcessedAt <= payment.DueDate)
                        {
                            rating += (int)(5 * recencyWeight);
                        }
                        else
                        {
                            rating -= (int)(8 * recencyWeight);
                        }
                    }
                    else if (payment.Status == PaymentStatus.Overdue)
                    {
                        rating -= (int)(5 * recencyWeight);
                    }
                }

                if (credit.Payments.Any(p => p.Status == PaymentStatus.Overdue))
                {
                    rating -= 30;
                }

                if (credit.Status == CreditStatus.Rejected)
                {
                    rating -= 20;
                }
            }
            return Math.Clamp(rating, 0, 1000);
        }
    }
}