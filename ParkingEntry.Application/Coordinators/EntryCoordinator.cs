using Mediator;
using Microsoft.Extensions.Logging;
using ParkingEntry.Application.Infrastructure;
using ParkingEntry.Application.Services.Commands;
using ParkingEntry.Application.StateMachine;
using ParkingEntry.Domain.Enums;
using ParkingEntry.Domain.Events;

namespace ParkingEntry.Application.Coordinators;

/// <summary>
/// Coordinates the entry flow by orchestrating state machine transitions and hardware commands.
/// </summary>
public sealed class EntryCoordinator
{
    private readonly IEntryStateMachine _stateMachine;
    private readonly IMediator _mediator;
    private readonly ITimeoutManager _timeoutManager;
    private readonly ILogger<EntryCoordinator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryCoordinator"/> class.
    /// </summary>
    /// <param name="stateMachine">The entry state machine.</param>
    /// <param name="mediator">The mediator instance.</param>
    /// <param name="timeoutManager">The timeout manager.</param>
    /// <param name="logger">The logger instance.</param>
    public EntryCoordinator(
        IEntryStateMachine stateMachine,
        IMediator mediator,
        ITimeoutManager timeoutManager,
        ILogger<EntryCoordinator> logger)
    {
        _stateMachine = stateMachine;
        _mediator = mediator;
        _timeoutManager = timeoutManager;
        _logger = logger;
    }

    /// <summary>
    /// Handles a domain event and orchestrates the appropriate actions.
    /// </summary>
    /// <param name="event">The domain event to handle.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task HandleEventAsync(
        DomainEvent @event,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "Handling event {EventType} in state {State}",
            @event.GetType().Name,
            _stateMachine.CurrentState);

        try
        {
            await ProcessEventAsync(@event, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing event {EventType}", @event.GetType().Name);
            await _stateMachine.TransitionToAsync(EntryState.Error, ct);
        }
    }

    private async Task ProcessEventAsync(DomainEvent @event, CancellationToken ct)
    {
        switch (@event)
        {
            case VehicleApproached:
                await HandleVehicleApproachedAsync(ct);
                break;

            case CardRead cardRead:
                await HandleCardReadAsync(cardRead, ct);
                break;

            case VehiclePassed:
                await HandleVehiclePassedAsync(ct);
                break;

            case VehicleReversed:
                await HandleVehicleReversedAsync(ct);
                break;

            case EquipmentHealthChanged healthChanged:
                await HandleHealthChangedAsync(healthChanged, ct);
                break;
        }

        await _stateMachine.TransitionAsync(@event, ct);
    }

    private async Task HandleVehicleApproachedAsync(CancellationToken ct)
    {
        // Timeout for card reading
        await _timeoutManager.StartTimeoutAsync(
            "CardReading",
            TimeSpan.FromSeconds(30),
            async () =>
            {
                _logger.LogWarning("Card reading timeout");
                await _stateMachine.TransitionToAsync(EntryState.Idle, ct);
            },
            ct);
    }

    private async Task HandleCardReadAsync(CardRead cardRead, CancellationToken ct)
    {
        _timeoutManager.CancelTimeout("CardReading");

        // Check access
        var accessResult = await _mediator.Send(
            new CheckAccessCommand(cardRead.CardNumber),
            ct);

        if (accessResult.IsAllowed)
        {
            // Check debt
            var debt = await _mediator.Send(
                new CalculateDebtCommand(cardRead.CardNumber),
                ct);

            if (debt > 0)
            {
                _logger.LogWarning(
                    "Client {CardNumber} has debt {Debt}",
                    cardRead.CardNumber,
                    debt);
            }

            await _stateMachine.TransitionToAsync(EntryState.OpeningBarrier, ct);
            await _stateMachine.TransitionToAsync(EntryState.WaitingPassage, ct);

            // Timeout for vehicle passage
            await _timeoutManager.StartTimeoutAsync(
                "VehiclePassage",
                TimeSpan.FromMinutes(2),
                async () =>
                {
                    _logger.LogWarning("Vehicle passage timeout");
                    await _mediator.Send(new CloseBarrierCommand(), ct);
                    await _stateMachine.TransitionToAsync(EntryState.Idle, ct);
                },
                ct);
        }
        else
        {
            _logger.LogWarning(
                "Access denied for {CardNumber}: {Reason}",
                cardRead.CardNumber,
                accessResult.Reason);

            await _stateMachine.TransitionToAsync(EntryState.Idle, ct);
        }
    }

    private async Task HandleVehiclePassedAsync(CancellationToken ct)
    {
        _timeoutManager.CancelTimeout("VehiclePassage");
        await _mediator.Send(new CloseBarrierCommand(), ct);
    }

    private async Task HandleVehicleReversedAsync(CancellationToken ct)
    {
        _timeoutManager.CancelTimeout("VehiclePassage");
        await _mediator.Send(new CloseBarrierCommand(), ct);
    }

    private async Task HandleHealthChangedAsync(
        EquipmentHealthChanged healthChanged,
        CancellationToken ct)
    {
        _logger.LogWarning(
            "Equipment {Equipment} health changed: {Status} - {Error}",
            healthChanged.EquipmentName,
            healthChanged.Status,
            healthChanged.ErrorMessage);

        if (healthChanged.Status == HealthStatus.Critical)
        {
            await _stateMachine.TransitionToAsync(EntryState.Error, ct);
        }
    }
}
