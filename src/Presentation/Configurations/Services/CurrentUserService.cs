using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Application.Interfaces;
using ServiceInyector.Interfaces;

namespace Presentation.Services;

public class CurrentUserService : ICurrentUserService, IRegisterAsSingleton
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
