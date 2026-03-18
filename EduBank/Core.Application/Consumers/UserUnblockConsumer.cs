using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Contracts.AuthServiceContracts;
using Core.Application.Services.Interfaces;
using MassTransit;

namespace Core.Application.Consumers
{
    public class UserUnblockConsumer : IConsumer<UnblockUserAccountsCommand>
    {
        private readonly IAccountService _accountService;

        public UserUnblockConsumer(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task Consume(ConsumeContext<UnblockUserAccountsCommand> context)
        {
            var result = await _accountService.UnblockAccountAsync(context.Message);
            await context.RespondAsync(result);
        }
    }
}
