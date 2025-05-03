// File: backend/FhirReceiverService/Services/FhirValidator.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;

namespace FhirReceiverService.Services;

public interface IFhirValidator
{
    Task<ValidationResult> ValidateAsync(Resource resource);
}

public class FhirValidator : IFhirValidator
{
    private readonly ILogger<FhirValidator> _logger;

    public FhirValidator(ILogger<FhirValidator> logger)
    {
        _logger = logger;
    }

    public Task<ValidationResult> ValidateAsync(Resource resource)
    {
        var result = new ValidationResult();
        
        try
        {
            // Validate based on resource type
            switch (resource)
            {
                case Patient patient:
                    ValidatePatient(patient, result);
                    break;
                    
                default:
                    result.Errors.Add(new ValidationError 
                    { 
                        Message = $"Unsupported resource type: {resource.TypeName}" 
                    });
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating FHIR resource");
            result.Errors.Add(new ValidationError { Message = $"Validation error: {ex.Message}" });
        }
        
        return System.Threading.Tasks.Task.FromResult(result);
    }
    
    private void ValidatePatient(Patient patient, ValidationResult result)
    {
        // Check for required fields for Ontario PCR
        if (string.IsNullOrEmpty(patient.Id))
        {
            result.Errors.Add(new ValidationError 
            { 
                Message = "Patient ID is required",
                Path = "Patient.id" 
            });
        }
        
        // Validate name
        if (patient.Name == null || patient.Name.Count == 0)
        {
            result.Errors.Add(new ValidationError 
            { 
                Message = "Patient name is required",
                Path = "Patient.name" 
            });
        }
        else
        {
            var name = patient.Name[0];
            
            if (string.IsNullOrEmpty(name.Family))
            {
                result.Errors.Add(new ValidationError 
                { 
                    Message = "Patient family name is required",
                    Path = "Patient.name.family" 
                });
            }
            
            if (name.Given == null || name.Given.Count() == 0)
            {
                result.Errors.Add(new ValidationError 
                { 
                    Message = "Patient given name is required",
                    Path = "Patient.name.given" 
                });
            }
        }
        
        // Validate birth date
        if (string.IsNullOrEmpty(patient.BirthDate))
        {
            result.Errors.Add(new ValidationError 
            { 
                Message = "Patient birth date is required",
                Path = "Patient.birthDate" 
            });
        }
        else
        {
            // Try to parse date to ensure it's valid
            try
            {
                var date = DateTime.Parse(patient.BirthDate);
                
                if (date > DateTime.Now)
                {
                    result.Errors.Add(new ValidationError 
                    { 
                        Message = "Patient birth date cannot be in the future",
                        Path = "Patient.birthDate" 
                    });
                }
            }
            catch
            {
                result.Errors.Add(new ValidationError 
                { 
                    Message = "Invalid patient birth date format",
                    Path = "Patient.birthDate" 
                });
            }
        }
        
        // Validate gender
        if (patient.Gender == null)
        {
            result.Errors.Add(new ValidationError 
            { 
                Message = "Patient gender is required",
                Path = "Patient.gender" 
            });
        }
    }
}

public class ValidationResult
{
    public List<ValidationError> Errors { get; } = new List<ValidationError>();
    
    public bool IsValid => Errors.Count == 0;
}

public class ValidationError
{
    public string Message { get; set; }
    public string Path { get; set; }
}
