// File: backend/TransmissionService/Services/IHl7Transmitter.cs
using System.Threading.Tasks;
using TransmissionService.Models;

namespace TransmissionService.Services;

public interface IHl7Transmitter
{
    Task<TransmissionResult> TransmitMessage(string messageId, string hl7Message);
}
