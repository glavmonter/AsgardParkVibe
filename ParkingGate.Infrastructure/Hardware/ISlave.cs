using ParkingGate.Domain.Models;

namespace ParkingGate.Infrastructure.Hardware;

public interface ISlave
{
    Task<BarrierStatus> OpenBarrierAsync(CancellationToken cancellationToken = default);
    Task<BarrierStatus> CloseBarrierAsync(CancellationToken cancellationToken = default);
    Task<LightStatus> SetLightAsync(LightStatus color, CancellationToken cancellationToken = default);
    Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default);
}
