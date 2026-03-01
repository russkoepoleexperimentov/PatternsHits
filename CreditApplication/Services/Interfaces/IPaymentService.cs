using Common.Contracts;
using CreditApplication.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditApplication.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<ProcessExternalPaymentResponse> ProcessExternalPaymentAsync(ProcessExternalPaymentCommand command);
        Task<IEnumerable<PaymentDto>> GetPaymentsByCreditIdAsync(Guid creditId);
        Task<PaymentDto> GetPaymentByIdAsync(Guid id);
    }
}
