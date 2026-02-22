using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> GetByIdAsync(Guid userId);
        Task UpdateAsync(Guid userId, UserUpdateDto request);
        Task SeedAsync();
        Task GiveUserRoleAsync(Guid userId, string role);
        Task RemoveUserRoleAsync(Guid userId, string role);
        Task<List<UserDto>> GetAllUsersAsync(string? query);
    }
}
