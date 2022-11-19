using Microsoft.AspNetCore.Diagnostics;
using Presentation.ExceptionHandler;
namespace Presentation;
public static class Pipeline
{
    public static void ConfigurePipeline(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseCors("CorsPolicy");
        app.UseHealthChecks("/health");
        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseExceptionHandler(c => c.Run(async context =>
        {
            var exception = context.Features
                .Get<IExceptionHandlerPathFeature>().Error;
            var handler = new GlobalExceptionHandler((IWebHostEnvironment)app.Services.GetService(typeof(IWebHostEnvironment)));

            await handler.HandleException(exception, context);

        }));
    }
}

