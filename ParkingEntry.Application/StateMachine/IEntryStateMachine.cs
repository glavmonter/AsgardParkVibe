using ParkingEntry.Domain.Enums;
using ParkingEntry.Domain.Events;

namespace ParkingEntry.Application.StateMachine;

/// <summary>
/// Defines the contract for the entry state machine.
/// </summary>
public interface IEntryStateMachine
{
    /// <summary>
    /// Gets the current state of the state machine.
    /// </summary>
    EntryState CurrentState { get; }

    /// <summary>
    /// Transitions the state machine based on a domain event.
    /// </summary>
    /// <param name="event">The domain event that triggers the transition.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The new state after transition.</returns>
    Task<EntryState> TransitionAsync(DomainEvent @event, CancellationToken ct);

    /// <summary>
    /// Directly transitions the state machine to a specific state.
    /// </summary>
    /// <param name="newState">The target state.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The new state after transition.</returns>
    Task<EntryState> TransitionToAsync(EntryState newState, CancellationToken ct);
}
