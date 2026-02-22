using Application.Dtos;
using Application.Services.Interfaces;
using AutoMapper;
using Common.Enums.Common.Enums;
using Common.Exceptions;
using Domain.Entities;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<UserUpdateDto> _updateValidator;
    private readonly IMapper _mapper;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public UserService(
        UserManager<ApplicationUser> userManager,
        IMapper mapper,
        IValidator<UserUpdateDto> updateValidator,
        RoleManager<IdentityRole<Guid>> roleManager)
    {
        _mapper = mapper;
        _userManager = userManager;
        _updateValidator = updateValidator;
        _roleManager = roleManager;
    }

    public async Task<UserDto> GetByIdAsync(Guid userId)
    {
        var user = await GetFromDbAsync(userId);
        return _mapper.Map<UserDto>(user);
    }

    public async Task UpdateAsync(Guid userId, UserUpdateDto request)
    {
        await _updateValidator.ValidateAndThrowAsync(request);

        var user = await GetFromDbAsync(userId);

        _mapper.Map(request, user);

        await _userManager.UpdateAsync(user);

    }

    public async Task SeedAsync()
    {
        string[] roles =
        {
            RoleNames.Customer,
            RoleNames.Employee,
            RoleNames.Admin
        };

        foreach (var role in roles)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }
    }

    public async Task GiveUserRoleAsync(Guid userId, string role)
    {
        var user = await GetFromDbAsync(userId);

        await _userManager.AddToRoleAsync(user, role);

    }

    public async Task RemoveUserRoleAsync(Guid userId, string role)
    {
        var user = await GetFromDbAsync(userId);

        await _userManager.RemoveFromRoleAsync(user, role);

    }

    public async Task<List<UserDto>> GetAllUsersAsync(string? query)
    {
        var usersQuery = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var lowered = query.ToLower();

            usersQuery = usersQuery.Where(u =>
                u.Credentials.ToLower().Contains(lowered));
        }

        var users = await usersQuery.ToListAsync();

        return _mapper.Map<List<UserDto>>(users);
    }



    private async Task<ApplicationUser> GetFromDbAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user == null)
            throw new NotFoundException("User not found");

        return user;
    }
}