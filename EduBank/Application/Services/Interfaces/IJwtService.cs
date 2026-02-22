using Application.Dtos;
using Domain.Entities;

namespace Application.Services.Abstractions
{
    public interface IJwtService
    {
        TokenResponse GenerateTokens(ApplicationUser user, List<string> roles);
    }
}