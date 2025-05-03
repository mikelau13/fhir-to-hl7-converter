// File: backend/ConversionService/Converters/AdtConverter.cs
using Hl7.Fhir.Model;
using System;
using System.Text;
using System.Threading.Tasks;

namespace ConversionService.Converters;

/// <summary>
/// FHIR to HL7 converter for ADT messages (A28, A31, A40).
/// Walidation for FHIR resources.
/// </summary>
public class AdtConverter
{
    private readonly IHl7MessageBuilder _messageBuilder;

    public AdtConverter(IHl7MessageBuilder messageBuilder)
    {
        _messageBuilder = messageBuilder;
    }

    public System.Threading.Tasks.Task<string> ConvertA28(Patient patient)
    {
        // A28 - Add Person Information
        var hl7Message = _messageBuilder
            .CreateMsh("ADT^A28", "Ontario")
            .AddPid(patient)
            .AddPv1("I") // Inpatient
            .AddMft(patient);

        return System.Threading.Tasks.Task.FromResult(hl7Message.ToString());
    }

    public System.Threading.Tasks.Task<string> ConvertA31(Patient patient)
    {
        // A31 - Update Person Information
        var hl7Message = _messageBuilder
            .CreateMsh("ADT^A31", "Ontario")
            .AddPid(patient)
            .AddPv1("I") // Inpatient 
            .AddMft(patient);

        return System.Threading.Tasks.Task.FromResult(hl7Message.ToString());
    }

    public System.Threading.Tasks.Task<string> ConvertA40(Patient patient)
    {
        // A40 - Merge Patient Information
        var hl7Message = _messageBuilder
            .CreateMsh("ADT^A40", "Ontario")
            .AddPid(patient)  // Current patient data
            .AddMrg(patient); // Merge information

        return System.Threading.Tasks.Task.FromResult(hl7Message.ToString());
    }
}

public interface IHl7MessageBuilder
{
    IHl7MessageBuilder CreateMsh(string messageType, string receivingApplication);
    IHl7MessageBuilder AddPid(Patient patient);
    IHl7MessageBuilder AddPv1(string patientClass);
    IHl7MessageBuilder AddMft(Patient patient);
    IHl7MessageBuilder AddMrg(Patient patient);
    string ToString();
}

public class Hl7MessageBuilder : IHl7MessageBuilder
{
    private readonly StringBuilder _messageBuilder = new();
    private readonly char _fieldSeparator = '|';
    private readonly char _componentSeparator = '^';
    
    public IHl7MessageBuilder CreateMsh(string messageType, string receivingApplication)
    {
        var now = DateTime.Now;
        var messageControlId = Guid.NewGuid().ToString();
        
        _messageBuilder.AppendLine($"MSH|^~\\&|FHIR_SYSTEM|CLINIC_ID|PCR|{receivingApplication}|{now:yyyyMMddHHmmss}||{messageType}|{messageControlId}|P|2.4");
        
        return this;
    }

    public IHl7MessageBuilder AddPid(Patient patient)
    {
        var pid = new StringBuilder();
        pid.Append("PID");
        pid.Append(_fieldSeparator);
        pid.Append("1"); // Set ID
        pid.Append(_fieldSeparator);
        pid.Append(patient.Id); // Patient ID
        pid.Append(_fieldSeparator);
        pid.Append(""); // Patient ID List
        pid.Append(_fieldSeparator);
        pid.Append(GetPatientName(patient)); // Patient Name
        pid.Append(_fieldSeparator);
        pid.Append(""); // Mother's Maiden Name
        pid.Append(_fieldSeparator);
        pid.Append(GetBirthDate(patient)); // Date of Birth
        pid.Append(_fieldSeparator);
        pid.Append(GetGender(patient)); // Gender
        pid.Append(_fieldSeparator);
        pid.Append(""); // Patient Alias
        pid.Append(_fieldSeparator);
        pid.Append(""); // Race
        pid.Append(_fieldSeparator);
        pid.Append(GetAddress(patient)); // Patient Address
        pid.Append(_fieldSeparator);
        pid.Append(""); // County Code
        pid.Append(_fieldSeparator);
        pid.Append(GetPhoneNumber(patient)); // Phone Number

        _messageBuilder.AppendLine(pid.ToString());
        return this;
    }

    public IHl7MessageBuilder AddPv1(string patientClass)
    {
        var pv1 = new StringBuilder();
        pv1.Append("PV1");
        pv1.Append(_fieldSeparator);
        pv1.Append("1"); // Set ID
        pv1.Append(_fieldSeparator);
        pv1.Append(patientClass); // Patient Class
        // Add other PV1 fields as needed

        _messageBuilder.AppendLine(pv1.ToString());
        return this;
    }

    public IHl7MessageBuilder AddMft(Patient patient)
    {
        var mft = new StringBuilder();
        mft.Append("MFT");
        mft.Append(_fieldSeparator);
        mft.Append("1"); // Set ID
        mft.Append(_fieldSeparator);
        mft.Append("MAT"); // Master File Type
        // Add other MFT fields as needed

        _messageBuilder.AppendLine(mft.ToString());
        return this;
    }

    public IHl7MessageBuilder AddMrg(Patient patient)
    {
        var mrg = new StringBuilder();
        mrg.Append("MRG");
        mrg.Append(_fieldSeparator);
        mrg.Append(patient.Id); // Prior Patient ID
        // Add other MRG fields as needed

        _messageBuilder.AppendLine(mrg.ToString());
        return this;
    }

    private string GetPatientName(Patient patient)
    {
        var name = patient.Name.FirstOrDefault();
        if (name == null) return "";
        
        return $"{name.Family}{_componentSeparator}{string.Join(" ", name.Given)}";
    }

    private string GetBirthDate(Patient patient)
    {
        if (string.IsNullOrEmpty(patient.BirthDate)) return "";
        
        return patient.BirthDate.Replace("-", "");
    }

    private string GetGender(Patient patient)
    {
        return patient.Gender?.ToString()[0].ToString() ?? "";
    }

    private string GetAddress(Patient patient)
    {
        var address = patient.Address?.FirstOrDefault();
        if (address == null) return "";
        
        var addressComponents = new[]
        {
            string.Join(" ", address.Line ?? Array.Empty<string>()),
            address.City,
            address.State,
            address.PostalCode
        };
        
        return string.Join(_componentSeparator.ToString(), addressComponents.Where(x => !string.IsNullOrEmpty(x)));
    }

    private string GetPhoneNumber(Patient patient)
    {
        var phone = patient.Telecom?.FirstOrDefault(t => t.System == ContactPoint.ContactPointSystem.Phone);
        return phone?.Value ?? "";
    }

    public override string ToString()
    {
        return _messageBuilder.ToString();
    }
}
