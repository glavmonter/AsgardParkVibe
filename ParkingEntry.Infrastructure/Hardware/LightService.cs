using Microsoft.Extensions.Logging;
using ParkingEntry.Domain.Enums;

namespace ParkingEntry.Infrastructure.Hardware;

/// <summary>
/// Implements traffic light control service.
/// </summary>
public sealed class LightService : ILightService
{
    private readonly ISlaveWorker _slaveWorker;
    private readonly ILogger<LightService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LightService"/> class.
    /// </summary>
    /// <param name="slaveWorker">The slave worker for hardware communication.</param>
    /// <param name="logger">The logger instance.</param>
    public LightService(
        ISlaveWorker slaveWorker,
        ILogger<LightService> logger)
    {
        _slaveWorker = slaveWorker;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<LightStatus> SwitchAsync(LightColor color, CancellationToken ct)
    {
        var command = color switch
        {
            LightColor.Red => "LIGHT_RED",
            LightColor.Green => "LIGHT_GREEN",
            LightColor.Both => "LIGHT_BOTH",
            LightColor.None => "LIGHT_OFF",
            _ => throw new ArgumentOutOfRangeException(nameof(color), color, "Invalid light color")
        };

        _logger.LogDebug("Switching light to {Color} with command {Command}", color, command);
        await _slaveWorker.SendCommandAsync(command, ct);

        return new LightStatus(color);
    }
}
