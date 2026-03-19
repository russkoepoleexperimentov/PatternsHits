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
                .ToListAsync();

            int rating = 500; 

            foreach (var credit in credits)
            {
                if (credit.Status == CreditStatus.Approved || credit.Status == CreditStatus.Closed)
                    rating += 30;

                if (credit.Status == CreditStatus.Closed)
                    rating += 50;

                var overdueCount = credit.Payments.Count(p => p.Status == PaymentStatus.Overdue);
                rating -= overdueCount * 15;

                if (credit.Payments.Any(p => p.Status == PaymentStatus.Overdue))
                    rating -= 20;
            }

            return Math.Clamp(rating, 0, 1000);
        }
    }
}