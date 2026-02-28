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
    [Route("api/credits")]
    public class CreditsController : ControllerBase
    {
        private readonly ICreditService _creditService;

        public CreditsController(ICreditService creditService)
        {
            _creditService = creditService;
        }

        private bool IsEmployeeOrAdmin() =>
            User.IsInRole(RoleNames.Employee);

        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [ProducesResponseType<List<CreditDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCredits([FromQuery] Guid? userId)
        {
            var currentUserId = HttpContext.GetUserId();
            var effectiveUserId = IsEmployeeOrAdmin() ? userId : currentUserId;
            var credits = await _creditService.GetCreditsAsync(effectiveUserId);
            return Ok(credits);
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [ProducesResponseType<CreditDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCredit(Guid id)
        {
            var credit = await _creditService.GetCreditByIdAsync(id);
            var currentUserId = HttpContext.GetUserId();
            if (!IsEmployeeOrAdmin() && credit.UserId != currentUserId)
                return Forbid();

            return Ok(credit);
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [ProducesResponseType<CreditDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateCredit(CreateCreditRequest request)
        {
            var currentUserId = HttpContext.GetUserId().Value;

            var credit = await _creditService.CreateCreditRequestAsync(request, currentUserId);

            return CreatedAtAction(nameof(GetCredit), new { id = credit.Id }, credit);
        }

        [HttpPatch("{id}/approve")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = $"{RoleNames.Employee}")]
        [ProducesResponseType<CreditDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ApproveCredit(Guid id, ApproveCreditRequest request)
        {
            var employeeId = HttpContext.GetUserId().Value;
            var credit = await _creditService.ApproveCreditAsync(id, request, employeeId);
            return Ok(credit);
        }

        [HttpPatch("{id}/reject")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = $"{RoleNames.Employee}")]
        [ProducesResponseType<CreditDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RejectCredit(Guid id, RejectCreditRequest request)
        {
             var employeeId = HttpContext.GetUserId().Value;
             var credit = await _creditService.RejectCreditAsync(id, request, employeeId);
             return Ok(credit);
        }
    }
}