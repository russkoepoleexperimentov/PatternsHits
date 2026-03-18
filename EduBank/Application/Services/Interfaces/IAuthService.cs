using Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IAuthService
    {
        Task RegisterAsync(UserRegisterDto dto);
        Task BlockUserAsync(Guid userId);
        Task UnblockUserAsync(Guid userId);
        Task ChangePasswordAsync(Guid userId, UserChangePassword dto);
    }
}
