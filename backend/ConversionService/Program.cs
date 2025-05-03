
// File: backend/ConversionService/Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Messaging.ServiceBus;
using Common.Services;
using ConversionService.Converters;
using ConversionService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure options
builder.Services.Configure<ServiceBusConnectionOptions>(
    builder.Configuration.GetSection("ServiceBus"));

// Add Service Bus client
builder.Services.AddSingleton(provider => 
{
    var connectionString = builder.Configuration.GetSection("ServiceBus:ConnectionString").Value;
    return new Azure.Messaging.ServiceBus.ServiceBusClient(connectionString);
});
builder.Services.AddSingleton<IServiceBusClient, Common.Services.ServiceBusClient>();

// Add application services
builder.Services.AddSingleton<IHl7MessageBuilder, Hl7MessageBuilder>();
builder.Services.AddScoped<AdtConverter>();

// Add hosted services
builder.Services.AddHostedService<FhirConsumerHost>();

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
