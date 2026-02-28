using Common;
using Core.Application.Dtos;
using Core.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Core.Web.Controllers
{
    [ApiController]
    [Route("api/v1/accounts")]
    [Authorize]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAccounts([FromQuery] Guid? userId)
        {
            var currentUserId = HttpContext.GetUserId()!.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            var accounts = await _accountService.GetAccountsAsync(userId, currentUserId, currentUserRole);
            return Ok(accounts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccount(Guid id)
        {
            var currentUserId = HttpContext.GetUserId()!.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            var account = await _accountService.GetAccountByIdAsync(id, currentUserId, currentUserRole);
            return Ok(account);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(CreateAccountDto dto)
        {
            var currentUserId = HttpContext.GetUserId()!.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            var account = await _accountService.CreateAccountAsync(dto, currentUserId, currentUserRole);
            return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> CloseAccount(Guid id)
        {
            var currentUserId = HttpContext.GetUserId()!.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            await _accountService.CloseAccountAsync(id, currentUserId, currentUserRole);
            return Ok();
        }

        [HttpGet("{id}/transactions")]
        public async Task<IActionResult> GetAccountTransactions(Guid id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var currentUserId = HttpContext.GetUserId()!.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            var transactions = await _accountService.GetAccountTransactionsAsync(id, from, to, currentUserId, currentUserRole);
            return Ok(transactions);
        }
    }
}
