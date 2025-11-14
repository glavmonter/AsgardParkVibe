using ParkingGate.Domain.Models;

namespace ParkingGate.Infrastructure.Hardware;

public interface IMifareReader
{
    Task<MifareStatus> StartSearchingAsync(CancellationToken cancellationToken = default);
    Task<MifareStatus> StopSearchingAsync(CancellationToken cancellationToken = default);
    Task<MifareStatus> StartWritingAsync(string cardNumber, string clientNumber, CancellationToken cancellationToken = default);
    Task<MifareStatus> StopWritingAsync(CancellationToken cancellationToken = default);
    Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);
}
