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
        private readonly ICreditRatingService _creditRatingService;
        public PaymentsController(IPaymentService paymentService, ICreditRatingService creditRatingService)
        {
            _paymentService = paymentService;
            _creditRatingService = creditRatingService;
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

        [HttpGet("user/{userId}/credit-rating")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetCreditRating(Guid userId)
        {
            var rating = await _creditRatingService.GetCreditRatingAsync(userId);
            return Ok(new { UserId = userId, Rating = rating });
        }

        [HttpGet("my/credit-rating")]
        public async Task<IActionResult> GetMyCreditRating()
        {
            var userId = HttpContext.GetUserId()!.Value;
            var rating = await _creditRatingService.GetCreditRatingAsync(userId);
            return Ok(new { Rating = rating });
        }

        [HttpGet("my/overdue")]
        public async Task<IActionResult> GetMyOverduePayments()
        {
            var userId = HttpContext.GetUserId()!.Value;
            var overdue = await _paymentService.GetOverdueByUserIdAsync(userId);
            return Ok(overdue);
        }

        [HttpGet("credit/{creditId}/overdue")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetOverdueByCredit(Guid creditId)
        {
            var overdue = await _paymentService.GetOverdueByCreditIdAsync(creditId);
            return Ok(overdue);
        }

        [HttpGet("user/{userId}/overdue")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetOverdueByUser(Guid userId)
        {
            var overdue = await _paymentService.GetOverdueByUserIdAsync(userId);
            return Ok(overdue);
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