using Common;
using Common.Enums;
using Common.Enums.Common.Enums;
using CreditApplication.Dtos;
using CreditApplication.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CreditService.Controllers
{
    [ApiController]
    [Route("api")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }


        [HttpGet("credits/{creditId}/payments")]
        [ProducesResponseType<List<PaymentDto>>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPaymentsByCredit(Guid creditId)
        {
            var currentUserId = HttpContext.GetUserId().Value;
            var payments = await _paymentService.GetPaymentsByCreditIdAsync(creditId);
            return Ok(payments);
        }

        [HttpGet("payments/{id}")]
        [ProducesResponseType<PaymentDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetPayment(Guid id)
        {
            var currentUserId = HttpContext.GetUserId().Value;
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            return Ok(payment);

        }
    }
}