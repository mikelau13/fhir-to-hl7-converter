// File: backend/ResendApiService/Controllers/ClinicsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ResendApiService.Models;

namespace ResendApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClinicsController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClinicsController> _logger;
    private readonly IOptions<ResendApiOptions> _options;

    public ClinicsController(
        HttpClient httpClient,
        ILogger<ClinicsController> logger,
        IOptions<ResendApiOptions> options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options;
    }
    
    [HttpGet]
    public async Task<IActionResult> GetClinics()
    {
        try
        {
            // Fetch clinics from logging service
            var clinics = await _httpClient.GetFromJsonAsync<List<ClinicModel>>(
                $"{_options.Value.LoggingServiceUrl}/api/clinics");
                
            return Ok(clinics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch clinics");
            return StatusCode(500, "Failed to fetch clinics");
        }
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetClinic(string id)
    {
        try
        {
            // Fetch clinic from logging service
            var clinic = await _httpClient.GetFromJsonAsync<ClinicModel>(
                $"{_options.Value.LoggingServiceUrl}/api/clinics/{id}");
                
            if (clinic == null)
                return NotFound();
                
            return Ok(clinic);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch clinic {ClinicId}", id);
            return StatusCode(500, "Failed to fetch clinic");
        }
    }
    
    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateClinicStatus(string id, [FromBody] ClinicStatusUpdateModel update)
    {
        try
        {
            // Update clinic status in logging service
            var response = await _httpClient.PutAsJsonAsync(
                $"{_options.Value.LoggingServiceUrl}/api/clinics/{id}/status",
                update);
                
            response.EnsureSuccessStatusCode();
            
            return Ok(new { success = true });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update status for clinic {ClinicId}", id);
            return StatusCode(500, "Failed to update clinic status");
        }
    }
}