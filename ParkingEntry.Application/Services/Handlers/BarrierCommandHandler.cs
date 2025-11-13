using Mediator;
using Microsoft.Extensions.Logging;
using ParkingEntry.Application.Services.Commands;
using ParkingEntry.Domain.Enums;
using ParkingEntry.Infrastructure.Hardware;

namespace ParkingEntry.Application.Services.Handlers;

/// <summary>
/// Handles barrier-related commands.
/// </summary>
public sealed class BarrierCommandHandler :
    ICommandHandler<OpenBarrierCommand, BarrierStatus>,
    ICommandHandler<CloseBarrierCommand, BarrierStatus>
{
    private readonly IBarrierService _barrierService;
    private readonly ILogger<BarrierCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="BarrierCommandHandler"/> class.
    /// </summary>
    /// <param name="barrierService">The barrier service.</param>
    /// <param name="logger">The logger instance.</param>
    public BarrierCommandHandler(
        IBarrierService barrierService,
        ILogger<BarrierCommandHandler> logger)
    {
        _barrierService = barrierService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async ValueTask<BarrierStatus> Handle(
        OpenBarrierCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation("Opening barrier");
        return await _barrierService.OpenAsync(ct);
    }

    /// <inheritdoc/>
    public async ValueTask<BarrierStatus> Handle(
        CloseBarrierCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation("Closing barrier");
        return await _barrierService.CloseAsync(ct);
    }
}
