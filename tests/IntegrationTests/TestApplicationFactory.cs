using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace IntegrationTests;

internal class TestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly List<Action<IServiceCollection>> _serviceListToOverride;

    public TestApplicationFactory(List<Action<IServiceCollection>> serviceListToOverride = null)
    {
        _serviceListToOverride = serviceListToOverride;
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        if (_serviceListToOverride is not null && _serviceListToOverride.Count > 0)
        {
            foreach (var service in _serviceListToOverride)
            {
                builder.ConfigureServices(service);
            }

        }

        builder.UseEnvironment("Testing");

        return base.CreateHost(builder);
    }
}

