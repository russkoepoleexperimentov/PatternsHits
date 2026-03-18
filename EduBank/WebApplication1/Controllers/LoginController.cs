using Application.Dtos;
using Duende.IdentityServer.Services;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Domain.Entities;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IValidator<UserLoginDto> _loginValidator;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        IIdentityServerInteractionService interaction,
        IValidator<UserLoginDto> loginValidator,
        ILogger<AccountController> logger)
    {
        _signInManager = signInManager;
        _interaction = interaction;
        _loginValidator = loginValidator;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string returnUrl)
    {
        _logger.LogInformation("GET Login called with returnUrl: {ReturnUrl}", returnUrl);
        var vm = new LoginViewModel { ReturnUrl = returnUrl ?? "~/" };
        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        _logger.LogInformation("POST Login called for email: {Email}", model.Email);

        var dto = new UserLoginDto { Email = model.Email, Password = model.Password };
        var validationResult = await _loginValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors)
                ModelState.AddModelError(string.Empty, error.ErrorMessage);
            return View(model);
        }

        var user = await _signInManager.UserManager.FindByEmailAsync(model.Email);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Неверный email или пароль");
            return View(model);
        }

        if (user.LockoutEnabled && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            ModelState.AddModelError(string.Empty, "Пользователь заблокирован");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

        if (result.Succeeded)
        {
            var returnUrl = model.ReturnUrl;
            if (string.IsNullOrEmpty(returnUrl) ||
                !(Url.IsLocalUrl(returnUrl) || _interaction.IsValidReturnUrl(returnUrl)))
            {
                returnUrl = "~/";
            }

            _logger.LogInformation("Login successful, redirecting to: {ReturnUrl}", returnUrl);
            return Redirect(returnUrl);
        }

        ModelState.AddModelError(string.Empty, "Неверный email или пароль");
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Logout(string logoutId)
    {
        _logger.LogInformation("Logout called with logoutId: {LogoutId}", logoutId);
        await _signInManager.SignOutAsync();

        var logoutContext = await _interaction.GetLogoutContextAsync(logoutId);
        var redirectUri = logoutContext?.PostLogoutRedirectUri ?? "~/";

        _logger.LogInformation("Redirecting after logout to: {RedirectUri}", redirectUri);
        return Redirect(redirectUri);
    }
}