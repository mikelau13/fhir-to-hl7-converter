// File: backend/TransmissionService/Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Messaging.ServiceBus;
using Common.Services;
using TransmissionService.Models;
using TransmissionService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure options
builder.Services.Configure<TransmissionOptions>(
    builder.Configuration.GetSection("Transmission"));
builder.Services.Configure<ServiceBusConnectionOptions>(
    builder.Configuration.GetSection("ServiceBus"));

// Add HTTP client
builder.Services.AddHttpClient<IHl7Transmitter, Hl7Transmitter>(client =>
{
    // Configure base URL for Ontario PCR endpoint
    var transmissionOptions = builder.Configuration.GetSection("Transmission").Get<TransmissionOptions>();
    if (transmissionOptions != null && !string.IsNullOrEmpty(transmissionOptions.EndpointUrl))
    {
        var uri = new Uri(transmissionOptions.EndpointUrl);
        client.BaseAddress = new Uri($"{uri.Scheme}://{uri.Host}");
    }
});

// Add Service Bus client
builder.Services.AddSingleton(provider => 
{
    var connectionString = builder.Configuration.GetSection("ServiceBus:ConnectionString").Value;
    return new Azure.Messaging.ServiceBus.ServiceBusClient(connectionString);
});
builder.Services.AddSingleton<IServiceBusClient, Common.Services.ServiceBusClient>();

// Add application services
builder.Services.AddScoped<IHl7Transmitter, Hl7Transmitter>();

// Add hosted services
builder.Services.AddHostedService<MessageConsumerHost>();
builder.Services.AddHostedService<RetryService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();