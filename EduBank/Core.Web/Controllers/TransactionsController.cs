using Common;
using Core.Application.Dtos;
using Core.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Core.Web.Controllers
{
    [ApiController]
    [Route("api/v1/transactions")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction(CreateTransactionDto dto)
        {
            var currentUserId = HttpContext.GetUserId()!.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            var transaction = await _transactionService.CreateTransactionAsync(dto, currentUserId, currentUserRole);
            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, transaction);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(Guid id)
        {
            var currentUserId = HttpContext.GetUserId()!.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            var transaction = await _transactionService.GetTransactionByIdAsync(id, currentUserId, currentUserRole);
            return Ok(transaction);
        }

        [HttpPost("{id}/comments")]
        public async Task<IActionResult> AddComment(Guid id, CreateCommentDto dto)
        {
            var authorId = HttpContext.GetUserId()!.Value;
            var comment = await _transactionService.AddCommentAsync(id, dto, authorId);
            return CreatedAtAction(nameof(GetComments), new { transactionId = id }, comment);
        }

        [HttpGet("{id}/comments")]
        public async Task<IActionResult> GetComments(Guid id, [FromQuery] Guid? parentId)
        {
            var comments = await _transactionService.GetCommentsAsync(id, parentId);
            return Ok(comments);
        }
    }
}
