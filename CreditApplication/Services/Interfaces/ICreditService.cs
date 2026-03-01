using CreditApplication.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditApplication.Services.Interfaces
{
    public interface ICreditService
    {
        Task<IEnumerable<CreditDto>> GetCreditsAsync(Guid? userId);
        Task<CreditDto> GetCreditByIdAsync(Guid id);
        Task<CreditDto> CreateCreditRequestAsync(CreateCreditRequest request, Guid currentUserId);
        Task<CreditDto> ApproveCreditAsync(Guid id, ApproveCreditRequest request, Guid employeeId);
        Task<CreditDto> RejectCreditAsync(Guid id, RejectCreditRequest request, Guid employeeId);
    }
}