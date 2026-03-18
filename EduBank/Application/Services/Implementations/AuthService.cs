using Application.Dtos;
using Application.Services.Abstractions;
using Application.Services.Interfaces;
using AutoMapper;
using Common.Contracts.AuthServiceContracts;
using Common.Enums.Common.Enums;
using Common.Exceptions;
using Domain.Entities;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<UserRegisterDto> _registerValidator;
    private readonly IValidator<UserChangePassword> _changeValidator;
    private readonly IMapper _mapper;
    private readonly IRequestClient<BlockUserAccountsCommand> _blockClient;
    private readonly IRequestClient<UnblockUserAccountsCommand> _unblockClient;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IValidator<UserRegisterDto> registerValidator,
        IValidator<UserChangePassword> changeValidator,
        IMapper mapper,
        IRequestClient<BlockUserAccountsCommand> blockClient,
        IRequestClient<UnblockUserAccountsCommand> unblockClient)
    {
        _userManager = userManager;
        _registerValidator = registerValidator;
        _changeValidator = changeValidator;
        _mapper = mapper;
        _blockClient = blockClient;
        _unblockClient = unblockClient;
    }

    public async Task RegisterAsync(UserRegisterDto dto)
    {
        await _registerValidator.ValidateAndThrowAsync(dto);

        if (await _userManager.FindByEmailAsync(dto.Email) != null)
            throw new EntryExistsException($"User with Email {dto.Email} already exists.");

        var user = _mapper.Map<ApplicationUser>(dto);
        user.UserName = $"user_{Guid.NewGuid()}";

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            throw new BadRequestException(
                string.Join(", ", result.Errors.Select(x => x.Description)));

        await _userManager.AddToRoleAsync(user, Roles.Customer.ToString());
    }

    public async Task BlockUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new NotFoundException("User not found");

        if (!user.LockoutEnabled)
            user.LockoutEnabled = true;

        user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            throw new BadRequestException(
                string.Join(", ", result.Errors.Select(e => e.Description)));

        var correlationId = Guid.NewGuid();

        var response = await _blockClient.GetResponse<BlockUserAccountsResponse>(
            new BlockUserAccountsCommand(userId, correlationId));

        if (!response.Message.Success)
        {
            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);

            throw new InvalidOperationException(
                $"Core service error: {response.Message.ErrorMessage}");
        }
    }

    public async Task UnblockUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw new NotFoundException("User not found");

        user.LockoutEnd = null;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
            throw new BadRequestException(
                string.Join(", ", result.Errors.Select(e => e.Description)));

        var correlationId = Guid.NewGuid();

        var response = await _unblockClient.GetResponse<UnblockUserAccountsResponse>(
            new UnblockUserAccountsCommand(userId, correlationId));

        if (!response.Message.Success)
        {
            await _userManager.UpdateAsync(user);

            throw new InvalidOperationException(
                $"Core service error: {response.Message.ErrorMessage}");
        }
    }

    public async Task ChangePasswordAsync(Guid userId, UserChangePassword dto)
    {
        await _changeValidator.ValidateAndThrowAsync(dto);

        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException("User not found");

        var result = await _userManager.ChangePasswordAsync(
            user,
            dto.OldPassword,
            dto.NewPassword);

        if (!result.Succeeded)
            throw new BadRequestException(
                string.Join(", ", result.Errors.Select(e => e.Description)));
    }
}