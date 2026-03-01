using Application.Dtos;
using Application.Services.Abstractions;
using Application.Services.Interfaces;
using AutoMapper;
using Common.Contracts.AuthServiceContracts;
using Common.Enums.Common.Enums;
using Common.Exceptions;
using Common.Options;
using Domain.Entities;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly UserDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly IValidator<UserRegisterDto> _registerValidator;
    private readonly IValidator<UserLoginDto> _loginValidator;
    private readonly IValidator<UserChangePassword> _changeValidator;
    private readonly IMapper _mapper;
    private readonly JwtOptions _jwtOptions;
    private readonly IRequestClient<BlockUserAccountsCommand> _blockClient;
    private readonly IRequestClient<UnblockUserAccountsCommand> _unblockClient;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        UserDbContext context,
        IJwtService jwtService,
        IValidator<UserRegisterDto> registerValidator,
        IValidator<UserLoginDto> loginValidator,
        IValidator<UserChangePassword> changeValidator,
        IMapper mapper,
        IOptions<JwtOptions> jwtOptions,
        IRequestClient<BlockUserAccountsCommand> blockClient,
        IRequestClient<UnblockUserAccountsCommand> unblockClient) 
    {
        _userManager = userManager;
        _context = context;
        _jwtService = jwtService;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _changeValidator = changeValidator;
        _mapper = mapper;
        _jwtOptions = jwtOptions.Value;
        _blockClient = blockClient;
        _unblockClient = unblockClient;
    }

    public async Task<TokenResponse> RegisterAsync(UserRegisterDto dto)
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

        var roles = await _userManager.GetRolesAsync(user);

        var tokens = _jwtService.GenerateTokens(user, roles.ToList());

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = tokens.RefreshToken,
            Expiration = DateTime.UtcNow.AddDays(_jwtOptions.Refresh.LifetimeDays)
        });

        await _context.SaveChangesAsync();

        return tokens;
    }

    public async Task<TokenResponse> LoginAsync(UserLoginDto dto)
    {
        await _loginValidator.ValidateAndThrowAsync(dto);

        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user == null ||
            !await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new BadRequestException("Wrong login or password");


        if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
            throw new BadRequestException("User is blocked");

        var roles = await _userManager.GetRolesAsync(user);

        var tokens = _jwtService.GenerateTokens(user, roles.ToList());

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = tokens.RefreshToken,
            Expiration = DateTime.UtcNow.AddDays(_jwtOptions.Refresh.LifetimeDays)
        });

        await _context.SaveChangesAsync();

        return tokens;
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
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        var correlationId = Guid.NewGuid();
        var response = await _blockClient.GetResponse<BlockUserAccountsResponse>(
            new BlockUserAccountsCommand(userId, correlationId));

        if (!response.Message.Success)
        {
            user.LockoutEnd = null;
            await _userManager.UpdateAsync(user);
            throw new InvalidOperationException($"Core service error: {response.Message.ErrorMessage}");
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
            throw new BadRequestException(string.Join(", ", result.Errors.Select(e => e.Description)));

        var correlationId = Guid.NewGuid();
        var response = await _unblockClient.GetResponse<UnblockUserAccountsResponse>(
            new UnblockUserAccountsCommand(userId, correlationId));

        if (!response.Message.Success)
        {
            await _userManager.UpdateAsync(user);
            throw new InvalidOperationException($"Core service error: {response.Message.ErrorMessage}");
        }
    }

    public async Task<TokenResponse> RefreshAsync(string refreshToken)
    {
        var tokenEntity = await _context.RefreshTokens
            .FirstOrDefaultAsync(x => x.Token == refreshToken);

        if (tokenEntity == null ||
            tokenEntity.IsRevoked ||
            tokenEntity.Expiration < DateTime.UtcNow)
            throw new SecurityTokenException("Invalid token");

        tokenEntity.IsRevoked = true;

        var user = await _userManager.FindByIdAsync(tokenEntity.UserId.ToString())
            ?? throw new NotFoundException("User not found");

        var roles = await _userManager.GetRolesAsync(user);

        var tokens = _jwtService.GenerateTokens(user, roles.ToList());

        _context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = tokens.RefreshToken,
            Expiration = DateTime.UtcNow.AddDays(_jwtOptions.Refresh.LifetimeDays)
        });

        await _context.SaveChangesAsync();

        return tokens;
    }

    public async Task LogoutAsync(Guid userId)
    {
        var tokens = await _context.RefreshTokens
            .Where(x => x.UserId == userId && !x.IsRevoked)
            .ToListAsync();

        foreach (var token in tokens)
            token.IsRevoked = true;

        await _context.SaveChangesAsync();
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