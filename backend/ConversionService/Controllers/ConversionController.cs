// File: backend/ConversionService/Controllers/ConversionController.cs
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ConversionService.Converters;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace ConversionService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConversionController : ControllerBase
{
    private readonly AdtConverter _adtConverter;
    private readonly FhirJsonParser _fhirParser;

    public ConversionController(AdtConverter adtConverter)
    {
        _adtConverter = adtConverter;
        _fhirParser = new FhirJsonParser();
    }

    [HttpPost("a28")]
    public async Task<IActionResult> ConvertA28([FromBody] string fhirResource)
    {
        try
        {
            var patient = _fhirParser.Parse<Patient>(fhirResource);
            var hl7Message = await _adtConverter.ConvertA28(patient);
            
            return Ok(new { hl7Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = $"Failed to convert FHIR to HL7: {ex.Message}" });
        }
    }

    [HttpPost("a31")]
    public async Task<IActionResult> ConvertA31([FromBody] string fhirResource)
    {
        try
        {
            var patient = _fhirParser.Parse<Patient>(fhirResource);
            var hl7Message = await _adtConverter.ConvertA31(patient);
            
            return Ok(new { hl7Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = $"Failed to convert FHIR to HL7: {ex.Message}" });
        }
    }

    [HttpPost("a40")]
    public async Task<IActionResult> ConvertA40([FromBody] string fhirResource)
    {
        try
        {
            var patient = _fhirParser.Parse<Patient>(fhirResource);
            var hl7Message = await _adtConverter.ConvertA40(patient);
            
            return Ok(new { hl7Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = $"Failed to convert FHIR to HL7: {ex.Message}" });
        }
    }
}
