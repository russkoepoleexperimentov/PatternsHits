using Common;
using Common.Enums.Common.Enums;
using Core.Application.Dtos;
using Core.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Core.Web.Controllers
{
    [ApiController]
    [Route("api/accounts")]
    [Authorize]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountsController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAccounts()
        {
            var currentUserId = HttpContext.GetUserId()!.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            var accounts = await _accountService.GetAccountsAsync(currentUserId, currentUserId);
            return Ok(accounts);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetAccount(Guid id)
        {
            var currentUserId = HttpContext.GetUserId()!.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            var account = await _accountService.GetAccountByIdAsync(id, currentUserId);
            return Ok(account);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAccount(CreateAccountDto dto)
        {
            var currentUserId = HttpContext.GetUserId()!.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            var account = await _accountService.CreateAccountAsync(currentUserId, dto);
            return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> CloseAccount(Guid id)
        {
            var currentUserId = HttpContext.GetUserId()!.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            await _accountService.CloseAccountAsync(id, currentUserId);
            return Ok();
        }

        [HttpGet("{id}/transactions")]
        [Authorize]
        public async Task<IActionResult> GetAccountTransactions(Guid id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            var currentUserId = HttpContext.GetUserId()!.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            var transactions = await _accountService.GetAccountTransactionsAsync(id, from, to, currentUserId);
            return Ok(transactions);
        }



        private bool IsEmployeeOrAdmin() =>
            User.IsInRole(RoleNames.Employee);

        [HttpGet("employee")]
        public async Task<IActionResult> GetAccountsEmployee()
        {
            if(!IsEmployeeOrAdmin()) return Forbid();
            var accounts = await _accountService.GetAllAccountsAsync();
            return Ok(accounts);
        }

        [HttpGet("employee/ofUser{id}")]
        public async Task<IActionResult> GetAccountsOfUserEmployee(Guid id)
        {
            if (!IsEmployeeOrAdmin()) return Forbid();
            var account = await _accountService.GetAccountsAsync(id, null);
            return Ok(account);
        }

        [HttpGet("employee/{id}")]
        public async Task<IActionResult> GetAccountEmployee(Guid id)
        {
            if (!IsEmployeeOrAdmin()) return Forbid();
            var account = await _accountService.GetAccountByIdAsync(id, null);
            return Ok(account);
        }

        [HttpDelete("employee/{id}")]
        [Authorize]
        public async Task<IActionResult> CloseAccountEmployee(Guid id)
        {
            if (!IsEmployeeOrAdmin()) return Forbid();
            await _accountService.CloseAccountAsync(id, null);
            return Ok();
        }

        [HttpGet("employee/{id}/transactions")]
        public async Task<IActionResult> GetAccountEmployeeTransactions(Guid id, [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            if (!IsEmployeeOrAdmin()) return Forbid();
            var transactions = await _accountService.GetAccountTransactionsAsync(id, from, to, null);
            return Ok(transactions);
        }
    }
}
