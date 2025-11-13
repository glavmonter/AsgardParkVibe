using Mediator;
using Microsoft.Extensions.Logging;
using ParkingEntry.Application.Services.Commands;
using ParkingEntry.Domain.Enums;
using ParkingEntry.Domain.Events;

namespace ParkingEntry.Application.StateMachine;

/// <summary>
/// Implements the entry state machine for managing parking entry flow.
/// </summary>
public sealed class EntryStateMachine : IEntryStateMachine
{
    private readonly IMediator _mediator;
    private readonly ILogger<EntryStateMachine> _logger;
    private EntryState _currentState = EntryState.Idle;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntryStateMachine"/> class.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    /// <param name="logger">The logger instance.</param>
    public EntryStateMachine(
        IMediator mediator,
        ILogger<EntryStateMachine> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public EntryState CurrentState => _currentState;

    /// <inheritdoc/>
    public async Task<EntryState> TransitionAsync(
        DomainEvent @event,
        CancellationToken ct)
    {
        var nextState = @event switch
        {
            VehicleApproached when _currentState == EntryState.Idle
                => EntryState.ReadingCard,

            CardRead when _currentState == EntryState.ReadingCard
                => EntryState.CheckingAccess,

            VehiclePassed when _currentState == EntryState.WaitingPassage
                => EntryState.Idle,

            VehicleReversed when _currentState == EntryState.WaitingPassage
                => EntryState.Idle,

            _ => _currentState
        };

        if (nextState != _currentState)
        {
            await TransitionToAsync(nextState, ct);
        }

        return _currentState;
    }

    /// <inheritdoc/>
    public async Task<EntryState> TransitionToAsync(
        EntryState newState,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "State transition: {From} -> {To}",
            _currentState,
            newState);

        await ExitStateAsync(_currentState, ct);
        _currentState = newState;
        await EnterStateAsync(_currentState, ct);

        return _currentState;
    }

    private async Task ExitStateAsync(EntryState state, CancellationToken ct)
    {
        switch (state)
        {
            case EntryState.ReadingCard:
                await _mediator.Send(new StopCardSearchCommand(), ct);
                break;
        }
    }

    private async Task EnterStateAsync(EntryState state, CancellationToken ct)
    {
        switch (state)
        {
            case EntryState.Idle:
                await _mediator.Send(new SwitchLightCommand(LightColor.Green), ct);
                await _mediator.Send(new CloseBarrierCommand(), ct);
                break;

            case EntryState.ReadingCard:
                await _mediator.Send(new SwitchLightCommand(LightColor.Red), ct);
                await _mediator.Send(new StartCardSearchCommand(), ct);
                break;

            case EntryState.OpeningBarrier:
                await _mediator.Send(new OpenBarrierCommand(), ct);
                break;
        }
    }
}
