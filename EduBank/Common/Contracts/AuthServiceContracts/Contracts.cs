using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Contracts.AuthServiceContracts
{
    public record BlockUserAccountsCommand(Guid UserId, Guid CorrelationId);
    public record BlockUserAccountsResponse(bool Success, string? ErrorMessage);

    public record UnblockUserAccountsCommand(Guid UserId, Guid CorrelationId);
    public record UnblockUserAccountsResponse(bool Success, string? ErrorMessage);
}
