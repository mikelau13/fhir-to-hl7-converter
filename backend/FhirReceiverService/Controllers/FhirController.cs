// File: backend/FhirReceiverService/Controllers/FhirController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Models;
using Common.Services;
//using FhirReceiverService.Models;
using FhirReceiverService.Services;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace FhirReceiverService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FhirController : ControllerBase
{
    private readonly IFhirValidator _validator;
    private readonly IServiceBusClient _serviceBusClient;
    private readonly ILogger<FhirController> _logger;
    private readonly FhirJsonParser _fhirParser;

    public FhirController(
        IFhirValidator validator,
        IServiceBusClient serviceBusClient,
        ILogger<FhirController> logger)
    {
        _validator = validator;
        _serviceBusClient = serviceBusClient;
        _logger = logger;
        _fhirParser = new FhirJsonParser();
    }
    
    [HttpPost]
    public async Task<IActionResult> ReceiveFhirResource([FromBody] string fhirResource)
    {
        if (string.IsNullOrWhiteSpace(fhirResource))
            return BadRequest("FHIR resource cannot be empty");

        try
        {
            // Parse the FHIR resource to validate it's valid FHIR
            var resource = _fhirParser.Parse<Resource>(fhirResource);
            
            // Determine the resource type
            var resourceType = resource.TypeName;
            
            // Validate the FHIR resource
            var validationResult = await _validator.ValidateAsync(resource);
            
            if (!validationResult.IsValid)
            {
                _logger.LogWarning("Invalid FHIR resource received: {ErrorCount} errors", 
                    validationResult.Errors.Count);
                    
                return BadRequest(new
                {
                    message = "Invalid FHIR resource",
                    errors = validationResult.Errors.Select(e => e.Message)
                });
            }
            
            // Extract MessageType based on resource type and content
            string messageType = DetermineMessageType(resource);
            if (string.IsNullOrEmpty(messageType))
            {
                _logger.LogWarning("Unsupported FHIR resource type: {ResourceType}", resourceType);
                return BadRequest(new { message = "Unsupported FHIR resource type" });
            }
            
            // Extract clinic and patient IDs
            var clinicId = ExtractClinicId(resource);
            var patientId = ExtractPatientId(resource);
            
            if (string.IsNullOrEmpty(clinicId))
            {
                _logger.LogWarning("Could not determine clinic ID from FHIR resource");
                return BadRequest(new { message = "Could not determine clinic ID" });
            }
            
            if (string.IsNullOrEmpty(patientId))
            {
                _logger.LogWarning("Could not determine patient ID from FHIR resource");
                return BadRequest(new { message = "Could not determine patient ID" });
            }
            
            // Generate a unique message ID
            var messageId = Guid.NewGuid().ToString();
            
            // Publish to service bus
            await _serviceBusClient.PublishFhirMessageAsync(
                messageId,
                fhirResource,
                clinicId,
                patientId,
                messageType);
                
            _logger.LogInformation("FHIR resource received and queued: {MessageId}, Type: {MessageType}", 
                messageId, messageType);
                
            return Ok(new 
            { 
                messageId, 
                status = "received", 
                message = "FHIR resource received and queued for processing" 
            });
        }
        catch (FormatException ex)
        {
            _logger.LogError(ex, "Invalid FHIR format");
            return BadRequest(new { message = "Invalid FHIR format", error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing FHIR resource");
            return StatusCode(500, new { message = "Error processing FHIR resource", error = ex.Message });
        }
    }
    
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new { status = "operational" });
    }
    
    private string DetermineMessageType(Resource resource)
    {
        // Implement logic to determine message type based on FHIR resource
        // For now, we only support Patient resources for ADT messages
        
        if (resource is Patient)
        {
            var patient = (Patient)resource;
            
            // Check for message type based on resource content
            // This is a simplified example, real logic would be more complex
            
            // A28 - Add person information
            if (patient.Meta?.Tag?.Any(t => 
                t.System == "http://terminology.hl7.org/CodeSystem/v2-0203" && 
                t.Code == "A28") == true)
            {
                return MessageType.A28;
            }
            
            // A31 - Update person information
            if (patient.Meta?.Tag?.Any(t => 
                t.System == "http://terminology.hl7.org/CodeSystem/v2-0203" && 
                t.Code == "A31") == true)
            {
                return MessageType.A31;
            }
            
            // A40 - Merge patient information
            if (patient.Meta?.Tag?.Any(t => 
                t.System == "http://terminology.hl7.org/CodeSystem/v2-0203" && 
                t.Code == "A40") == true)
            {
                return MessageType.A40;
            }
            
            // Default to A31 if no specific tag is found
            return MessageType.A31;
        }
        
        return null;
    }
    
    private string ExtractClinicId(Resource resource)
    {
        // Extract clinic ID from resource
        // This is a simplified example, real logic would be more complex
        
        if (resource is Patient patient)
        {
            // Try to get clinic ID from managingOrganization
            if (patient.ManagingOrganization != null)
            {
                var orgReference = patient.ManagingOrganization.Reference;
                if (!string.IsNullOrEmpty(orgReference) && orgReference.StartsWith("Organization/"))
                {
                    return orgReference.Substring("Organization/".Length);
                }
            }
            
            // Fallback to a tag or extension that might contain the clinic ID
            var clinicExtension = patient.Extension?.FirstOrDefault(e => 
                e.Url == "http://example.org/fhir/StructureDefinition/clinicId");
                
            if (clinicExtension?.Value != null)
            {
                return clinicExtension.Value.ToString();
            }
        }
        
        // Default clinic ID for testing purposes
        return "CLINIC001";
    }
    
    private string ExtractPatientId(Resource resource)
    {
        // Extract patient ID from resource
        // This is a simplified example, real logic would be more complex
        
        if (resource is Patient patient)
        {
            // Use the patient's ID
            if (!string.IsNullOrEmpty(patient.Id))
            {
                return patient.Id;
            }
            
            // Or try to get from an identifier
            var medicalRecordNumber = patient.Identifier?.FirstOrDefault(i => 
                i.System == "http://example.org/fhir/identifier/mrn");
                
            if (medicalRecordNumber?.Value != null)
            {
                return medicalRecordNumber.Value;
            }
        }
        
        // Generate a random ID for testing purposes
        return "P" + DateTime.Now.Ticks.ToString().Substring(10);
    }
}
