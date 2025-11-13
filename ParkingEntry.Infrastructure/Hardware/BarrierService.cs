using Microsoft.Extensions.Logging;
using ParkingEntry.Domain.Enums;

namespace ParkingEntry.Infrastructure.Hardware;

/// <summary>
/// Implements barrier control service.
/// </summary>
public sealed class BarrierService : IBarrierService
{
    private readonly ISlaveWorker _slaveWorker;
    private readonly ILogger<BarrierService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BarrierService"/> class.
    /// </summary>
    /// <param name="slaveWorker">The slave worker for hardware communication.</param>
    /// <param name="logger">The logger instance.</param>
    public BarrierService(
        ISlaveWorker slaveWorker,
        ILogger<BarrierService> logger)
    {
        _slaveWorker = slaveWorker;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<BarrierStatus> OpenAsync(CancellationToken ct)
    {
        _logger.LogDebug("Sending open command to barrier");
        await _slaveWorker.SendCommandAsync("BARRIER_OPEN", ct);
        return BarrierStatus.Open;
    }

    /// <inheritdoc/>
    public async Task<BarrierStatus> CloseAsync(CancellationToken ct)
    {
        _logger.LogDebug("Sending close command to barrier");
        await _slaveWorker.SendCommandAsync("BARRIER_CLOSE", ct);
        return BarrierStatus.Closed;
    }
}
