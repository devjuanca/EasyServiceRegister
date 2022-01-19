using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Presentation;
public static class InitialConfigurations
{
    public static async Task ApplySeeder(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        var services = scope.ServiceProvider;

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

        var currentUserService = scope.ServiceProvider.GetRequiredService<ICurrentUserService>();

        var context = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();

        try
        {
            if (context.Database.IsSqlServer())
            {
                await context.Database.MigrateAsync();
            }

            ApplicationDbContextSeed.RunSeeders(context); /*Configure your seeders*/
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            logger.LogError(ex, "An error occurred while migrating or seeding the database");

            throw;
        }
    }
}

