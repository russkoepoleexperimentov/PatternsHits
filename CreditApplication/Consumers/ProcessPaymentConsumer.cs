using Common.Contracts;
using CreditApplication.Services.Interfaces;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace CreditApplication.Consumers
{
    public class ProcessExternalPaymentConsumer : IConsumer<ProcessExternalPaymentCommand>
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<ProcessExternalPaymentConsumer> _logger;

        public ProcessExternalPaymentConsumer(IPaymentService paymentService, ILogger<ProcessExternalPaymentConsumer> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<ProcessExternalPaymentCommand> context)
        {
            _logger.LogInformation("Received external payment for credit {CreditId}, amount {Amount}",
                context.Message.CreditId, context.Message.Amount);

            var result = await _paymentService.ProcessExternalPaymentAsync(context.Message);
            await context.RespondAsync(result);
        }
    }
}