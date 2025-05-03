// File: backend/NotificationService/Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NotificationService.Models;
using NotificationService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure options
builder.Services.Configure<EmailOptions>(
    builder.Configuration.GetSection("EmailOptions"));
builder.Services.Configure<DigestOptions>(
    builder.Configuration.GetSection("DigestOptions"));

// Add HTTP client
builder.Services.AddHttpClient<IDigestGenerator, DigestGenerator>();

// Add application services
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IDigestGenerator, DigestGenerator>();

// Add background services
builder.Services.AddHostedService<DigestBackgroundService>();

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
