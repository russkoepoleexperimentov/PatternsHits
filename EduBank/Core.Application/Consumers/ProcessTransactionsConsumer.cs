using Core.Application.Dtos;
using Core.Application.Services.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Consumers
{
    public class ProcessTransactionConsumer : IConsumer<ProcessTransactionCommand>
    {
        private readonly ITransactionService _transactionService;

        public ProcessTransactionConsumer(
            ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        public async Task Consume(ConsumeContext<ProcessTransactionCommand> context)
        {
            var command = context.Message;

            var existing = await _transactionService.GetTransactionByIdIfExistsAsync(command.TransactionId);
            if (existing != null)
            {
                await context.RespondAsync(new ProcessTransactionResponse(true, null, existing));
                return;
            }

            try
            {
                var result = await _transactionService.ExecuteTransactionAsync(command.Dto, command.CurrentUserId, command.TransactionId);
                await context.RespondAsync(new ProcessTransactionResponse(true, null, result));
            }
            catch (Exception ex)
            {
                await context.RespondAsync(new ProcessTransactionResponse(false, ex.Message, null));
            }
        }
    }
}
