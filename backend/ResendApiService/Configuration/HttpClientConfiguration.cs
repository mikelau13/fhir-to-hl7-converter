// File: backend/ResendApiService/Configuration/HttpClientConfiguration.cs
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ResendApiService.Models;
using System.Net.Http;

namespace ResendApiService.Configuration;

public static class HttpClientConfiguration
{
    public static IServiceCollection AddHttpClientServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register named HTTP clients for different services
        services.AddHttpClient("LoggingService", (provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<ResendApiOptions>>().Value;
            client.BaseAddress = new Uri(options.LoggingServiceUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        
        services.AddHttpClient("TransmissionService", (provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<ResendApiOptions>>().Value;
            client.BaseAddress = new Uri(options.TransmissionServiceUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        
        return services;
    }
}