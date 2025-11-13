using Mediator;
using Microsoft.Extensions.Logging;
using ParkingEntry.Application.Services.Commands;
using ParkingEntry.Domain.Enums;
using ParkingEntry.Infrastructure.Hardware;

namespace ParkingEntry.Application.Services.Handlers;

/// <summary>
/// Handles traffic light commands.
/// </summary>
public sealed class LightCommandHandler :
    ICommandHandler<SwitchLightCommand, LightStatus>
{
    private readonly ILightService _lightService;
    private readonly ILogger<LightCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LightCommandHandler"/> class.
    /// </summary>
    /// <param name="lightService">The light service.</param>
    /// <param name="logger">The logger instance.</param>
    public LightCommandHandler(
        ILightService lightService,
        ILogger<LightCommandHandler> logger)
    {
        _lightService = lightService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async ValueTask<LightStatus> Handle(
        SwitchLightCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation("Switching light to {Color}", command.Color);
        return await _lightService.SwitchAsync(command.Color, ct);
    }
}
