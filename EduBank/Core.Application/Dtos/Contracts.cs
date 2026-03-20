using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Dtos
{
    public record ProcessTransactionCommand(
        Guid TransactionId,
        CreateTransactionDto Dto,
        Guid CurrentUserId
    );

    public record ProcessTransactionResponse(
    bool Success,
    string? ErrorMessage,
    TransactionDto? Transaction
    );
}
