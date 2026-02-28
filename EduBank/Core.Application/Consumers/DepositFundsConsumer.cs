using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Contracts;
using Core.Application.Services.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Core.Application.Consumers
{
    public class DepositFundsConsumer : IConsumer<DepositFundsCommand>
    {
        private readonly ITransactionService _transactionService;

        public DepositFundsConsumer(ITransactionService paymentService)
        {
            _transactionService = paymentService;
        }

        public async Task Consume(ConsumeContext<DepositFundsCommand> context)
        {
            var result = await _transactionService.ProcessDepositFund(context.Message);
            await context.RespondAsync(result);
        }
    }
}
