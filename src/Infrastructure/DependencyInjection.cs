using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Tech.CleanArchitecture.Infrastructure.Persistence.Services;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("UseInMemoryDatabase"))
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(configuration["Database:InMemory"]));
        }
        else
        {

            services.AddDbContextPool<ApplicationDbContext>(opt => opt.UseSqlServer(configuration["Database:Default"],
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        }

        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
        //services.AddTransient<IDateTime, DateTimeService>();
        //services.AddTransient<IDomainEventService, DomainEventService>();

        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
          .AddJwtBearer(cfg =>
          {
              //AUTHORITY
              cfg.Authority = configuration["Jwt:Authority"];
              cfg.Audience = configuration["Jwt:Audience"];
              cfg.IncludeErrorDetails = true;
              cfg.RequireHttpsMetadata = false;
              cfg.TokenValidationParameters = new TokenValidationParameters()
              {
                  ValidateAudience = false,
                  ValidateIssuerSigningKey = true,
                  ValidateIssuer = true,
                  //ISSUER
                  ValidIssuer = configuration["Jwt:Authority"],
                  ValidateLifetime = true
              };

              cfg.Events = new JwtBearerEvents
              {
                  OnAuthenticationFailed = c =>
                  {
                      c.NoResult();
                      c.Response.StatusCode = 401;
                      c.Response.ContentType = "text/plain";
                      return c.Response.WriteAsync(c.Exception.ToString());
                  }
              };
          });

        return services;
    }
}

