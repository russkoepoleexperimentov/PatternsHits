using AutoMapper;
using Common.Enums;
using CreditApplication.Dtos;
using CreditApplication.Services.Interfaces;
using CreditDomain.Entities;
using CreditInfrastructure;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CreditService.Services
{
    public class TariffService : ITariffService
    {
        private readonly CreditDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<TariffService> _logger;
        private readonly IValidator<CreateTariffRequest> _createValidator;

        public TariffService(
            CreditDbContext context,
            IMapper mapper,
            ILogger<TariffService> logger,
            IValidator<CreateTariffRequest> createValidator)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
            _createValidator = createValidator;
        }

        public async Task<IEnumerable<TariffDto>> GetAllTariffsAsync()
        {
            var tariffs = await _context.Tariffs.ToListAsync();
            return _mapper.Map<IEnumerable<TariffDto>>(tariffs);
        }

        public async Task<TariffDto> GetTariffByIdAsync(Guid id)
        {
            var tariff = await _context.Tariffs.FindAsync(id);
            if (tariff == null)
                throw new KeyNotFoundException($"Tariff with id {id} not found.");

            return _mapper.Map<TariffDto>(tariff);
        }

        public async Task<TariffDto> CreateTariffAsync(CreateTariffRequest request)
        {
            await _createValidator.ValidateAndThrowAsync(request);

            var tariff = _mapper.Map<Tariff>(request);
            _context.Tariffs.Add(tariff);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tariff created: {TariffId}", tariff.Id);
            return _mapper.Map<TariffDto>(tariff);
        }

        public async Task<TariffDto> UpdateTariffAsync(Guid id, UpdateTariffRequest request)
        {
            await _createValidator.ValidateAndThrowAsync(request);

            var tariff = await _context.Tariffs.FindAsync(id);
            if (tariff == null)
                throw new KeyNotFoundException($"Tariff with id {id} not found.");

            _mapper.Map(request, tariff);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tariff updated: {TariffId}", tariff.Id);
            return _mapper.Map<TariffDto>(tariff);
        }

        public async Task DeleteTariffAsync(Guid id)
        {
            var tariff = await _context.Tariffs
                .Include(t => t.Credits)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tariff == null)
                throw new KeyNotFoundException($"Tariff with id {id} not found.");

            if (tariff.Credits.Any(c => c.Status == CreditStatus.Approved || c.Status == CreditStatus.Pending))
                throw new InvalidOperationException("Cannot delete tariff with active credits.");

            _context.Tariffs.Remove(tariff);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Tariff deleted: {TariffId}", id);
        }
    }
}