using Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using EasyServiceRegister.Attributes;
using System.Security.Claims;

namespace Presentation.Services;

[RegisterAsSingleton]
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string UserId => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
    public string Email => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);
    public string Name => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.GivenName);
    public Task<string> JwtTokenAsync => _httpContextAccessor.HttpContext?.GetTokenAsync("Bearer", "access_token");
}
