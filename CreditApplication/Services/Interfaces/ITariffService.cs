using CreditApplication.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CreditApplication.Services.Interfaces
{
    public interface ITariffService
    {
        Task<IEnumerable<TariffDto>> GetAllTariffsAsync();
        Task<TariffDto> GetTariffByIdAsync(Guid id);
        Task<TariffDto> CreateTariffAsync(CreateTariffRequest request);
        Task<TariffDto> UpdateTariffAsync(Guid id, UpdateTariffRequest request);
        Task DeleteTariffAsync(Guid id);
    }

}
