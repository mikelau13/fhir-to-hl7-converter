// File: backend/FhirReceiverService/Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Common.Services;
using FhirReceiverService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure options
builder.Services.Configure<ServiceBusConnectionOptions>(
    builder.Configuration.GetSection("ServiceBus"));

// Add application services
builder.Services.AddScoped<IFhirValidator, FhirValidator>();
builder.Services.AddSingleton<IServiceBusClient, ServiceBusClient>();

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