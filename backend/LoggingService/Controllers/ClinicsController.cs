// File: backend/LoggingService/Controllers/ClinicsController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LoggingService.Data;
using LoggingService.Models;

namespace LoggingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClinicsController : ControllerBase
{
    private readonly LoggingDbContext _context;
    private readonly ILogger<ClinicsController> _logger;

    public ClinicsController(
        LoggingDbContext context,
        ILogger<ClinicsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetClinics()
    {
        var clinics = await _context.Clinics.ToListAsync();
        return Ok(clinics);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClinic(string id)
    {
        var clinic = await _context.Clinics.FindAsync(id);
        
        if (clinic == null)
            return NotFound();
            
        return Ok(clinic);
    }

    [HttpPost]
    public async Task<IActionResult> CreateClinic([FromBody] Clinic clinic)
    {
        if (string.IsNullOrEmpty(clinic.ClinicId))
            clinic.ClinicId = Guid.NewGuid().ToString();
            
        _context.Clinics.Add(clinic);
        await _context.SaveChangesAsync();
        
        return CreatedAtAction(nameof(GetClinic), new { id = clinic.ClinicId }, clinic);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClinic(string id, [FromBody] Clinic clinic)
    {
        if (id != clinic.ClinicId)
            return BadRequest();
            
        _context.Entry(clinic).State = EntityState.Modified;
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ClinicExists(id))
                return NotFound();
            else
                throw;
        }
        
        return NoContent();
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateClinicStatus(string id, [FromBody] ClinicStatusUpdateModel update)
    {
        var clinic = await _context.Clinics.FindAsync(id);
        
        if (clinic == null)
            return NotFound();
            
        clinic.IsActive = update.IsActive;
        
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ClinicExists(id))
                return NotFound();
            else
                throw;
        }
        
        return Ok(new { success = true });
    }

    private bool ClinicExists(string id)
    {
        return _context.Clinics.Any(e => e.ClinicId == id);
    }
}

public class ClinicStatusUpdateModel
{
    public bool IsActive { get; set; }
}
