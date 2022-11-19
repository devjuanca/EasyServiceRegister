using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Application.Interfaces;
using Application.Security;

namespace Application.Behaviours;

public class AuthorizationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAppSettingsService _appSettingsService;

    public AuthorizationBehaviour(
        ICurrentUserService currentUserService, IAppSettingsService appSettingsService)
    {
        _currentUserService = currentUserService;
        _appSettingsService = appSettingsService;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        var authorizeAttribute = request?.GetType().GetCustomAttribute<AuthorizeAttribute>();

        if (authorizeAttribute != null)
        {
            // Must be authenticated user
            if (_currentUserService.UserId == null)
            {
                throw new UnauthorizedAccessException();
            }

            if (!string.IsNullOrEmpty(authorizeAttribute.Roles))
            {
                string token = await _currentUserService.JwtTokenAsync;

                string url = $"{_appSettingsService["AuthUrl"]}/api/Modules/AuthorizeUserInModuleWithRoles?ModuleCode={_appSettingsService["ModuleCode"]}";

                authorizeAttribute.Roles.Split(";").ToList().ForEach(r => url += $"&Roles={r}");

                using HttpClient client = new();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                HttpResponseMessage response = await client.GetAsync(url, cancellationToken);

                var content = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.StatusCode != HttpStatusCode.OK || !bool.Parse(content))
                    throw new UnauthorizedAccessException();
            }
        }

        // User is authorized / authorization not required
        return await next();
    }
}

