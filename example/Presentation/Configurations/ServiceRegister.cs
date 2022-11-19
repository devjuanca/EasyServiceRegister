using Application;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Infrastructure;
using Infrastructure.Persistence;
using Presentation.Services;
namespace Presentation;
public static class ServiceRegister
{
    public static void AddServices(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(configuration);
        //builder.Services.AddSingleton<ICurrentUserService, CurrentUserService>();
        //builder.Services.AddSingleton<IAppSettingsService, AppSettingsService>();

        builder.Services.AddHttpContextAccessor();
        builder.Services.AddHealthChecks()
                        .AddDbContextCheck<ApplicationDbContext>();



        // Customise default API behaviour
        builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", builder => builder
                .WithOrigins(configuration.GetSection("Settings:CorsAllowedDomains")?.Value.Split(";"))
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
        });
    }
}

