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
    [Route("api/tariffs")]
    public class TariffsController : ControllerBase
    {
        private readonly ITariffService _tariffService;

        public TariffsController(ITariffService tariffService)
        {
            _tariffService = tariffService;
        }


        [HttpGet]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [ProducesResponseType<List<TariffDto>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTariffs()
        {
            var tariffs = await _tariffService.GetAllTariffsAsync();
            return Ok(tariffs);
        }


        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer")]
        [ProducesResponseType<TariffDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTariff(Guid id)
        {
            var tariff = await _tariffService.GetTariffByIdAsync(id);
            return Ok(tariff);
        }


        [HttpPost]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = $"{RoleNames.Employee}")]
        [ProducesResponseType<TariffDto>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateTariff(CreateTariffRequest request)
        {
            var tariff = await _tariffService.CreateTariffAsync(request);
            return CreatedAtAction(nameof(GetTariff), new { id = tariff.Id }, tariff);
        }


        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = $"{RoleNames.Employee}")]
        [ProducesResponseType<TariffDto>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateTariff(Guid id, UpdateTariffRequest request)
        {

            var tariff = await _tariffService.UpdateTariffAsync(id, request);
            return Ok(tariff);
        }

        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = $"{RoleNames.Employee}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteTariff(Guid id)
        {
            await _tariffService.DeleteTariffAsync(id);
            return Ok();
            

        }
    }
}