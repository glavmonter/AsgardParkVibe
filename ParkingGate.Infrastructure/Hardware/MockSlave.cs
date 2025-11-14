using Microsoft.Extensions.Logging;
using ParkingGate.Domain.Models;

namespace ParkingGate.Infrastructure.Hardware;

public class MockSlave : ISlave
{
    private readonly ILogger<MockSlave> _logger;
    private BarrierStatus _currentBarrierStatus = BarrierStatus.Closed;
    private LightStatus _currentLightStatus = LightStatus.Green;

    public MockSlave(ILogger<MockSlave> logger)
    {
        _logger = logger;
    }

    public async Task<BarrierStatus> OpenBarrierAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Opening barrier");
        await Task.Delay(500, cancellationToken);
        _currentBarrierStatus = BarrierStatus.Open;
        return _currentBarrierStatus;
    }

    public async Task<BarrierStatus> CloseBarrierAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Closing barrier");
        await Task.Delay(500, cancellationToken);
        _currentBarrierStatus = BarrierStatus.Closed;
        return _currentBarrierStatus;
    }

    public async Task<LightStatus> SetLightAsync(LightStatus color, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Setting light to {Color}", color);
        await Task.Delay(100, cancellationToken);
        _currentLightStatus = color;
        return _currentLightStatus;
    }

    public Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthStatus.Healthy);
    }
}
