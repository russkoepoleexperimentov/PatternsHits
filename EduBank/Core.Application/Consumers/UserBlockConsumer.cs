
using Common.Contracts.AuthServiceContracts;
using Core.Application.Services.Interfaces;
using MassTransit;

namespace Core.Application.Consumers
{
    public class UserBlockConsumer : IConsumer<BlockUserAccountsCommand>
    {

        private readonly IAccountService _accountService;

        public UserBlockConsumer(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task Consume(ConsumeContext<BlockUserAccountsCommand> context)
        {
            var result = await _accountService.BlockAccountAsync(context.Message);
            await context.RespondAsync(result);
        }
    }
}
