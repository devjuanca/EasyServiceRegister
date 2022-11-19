using Application.Interfaces;
using Serilog;
using Serilog.Sinks.Elasticsearch;

namespace Presentation.Configurations
{
    public static class SerilogRegister
    {
        public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder, IConfiguration configuration)
        {
            var currentUserService = builder.Services.BuildServiceProvider().GetService<ICurrentUserService>();


            Log.Logger = new LoggerConfiguration()
                   .ReadFrom.Configuration(configuration)
                   .Enrich.WithAssemblyName()
                   .Enrich.WithClientIp()
                   .Enrich.WithClientAgent()
                   .Enrich.WithEnvironmentName()
                   .WriteTo.Console(
                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{SourceContext}] [{EventId}] {Message}{Properties}{NewLine}{Exception}"
                            )
                   .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(
                            node: new Uri(configuration["LoggingOptions:ElasticUrl"]))
                   {
                       AutoRegisterTemplate = true,
                       IndexFormat = $"{configuration["IndexHeader"]}-Logs-{DateTime.Now:dd-mm-yyyy}"
                   })
                   .CreateLogger();

            builder.Logging.AddSerilog();

            return builder;
        }
    }
}
