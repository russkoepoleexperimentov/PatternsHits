using Dtos;

namespace Services.Interfaces
{
    public interface IOptionsService
    {
        Task<OptionsDto> GetOptionsAsync(Guid userId);
        Task<OptionsDto> UpdateOptionsAsync(Guid userId, OptionsDto dto);
        Task<OptionsDto> UpdateWebThemeAsync(Guid userId, string theme);
        Task<OptionsDto> UpdateMobileThemeAsync(Guid userId, string theme);
        Task<OptionsDto> AddHiddenAccountAsync(Guid userId, Guid accountId);
        Task<OptionsDto> RemoveHiddenAccountAsync(Guid userId, Guid accountId);
    }
}
