using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditApplication.Services.Interfaces
{
    public interface ICreditRatingService
    {
        Task<int> GetCreditRatingAsync(Guid userId);
    }
}