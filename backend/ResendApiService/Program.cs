// File: backend/ResendApiService/Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Common.Services;
using ResendApiService.Models;
using ResendApiService.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

// Configure options
builder.Services.Configure<ResendApiOptions>(
    builder.Configuration.GetSection("ResendApiOptions"));
builder.Services.Configure<ServiceBusConnectionOptions>(
    builder.Configuration.GetSection("ServiceBus"));

// Add HTTP client services
builder.Services.AddHttpClientServices(builder.Configuration);

// Add service bus client
builder.Services.AddSingleton<IServiceBusClient, ServiceBusClient>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(options => options
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
