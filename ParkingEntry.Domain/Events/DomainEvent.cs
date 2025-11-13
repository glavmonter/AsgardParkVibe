namespace ParkingEntry.Domain.Events;

/// <summary>
/// Base class for all domain events.
/// </summary>
public abstract record DomainEvent
{
    /// <summary>
    /// Gets the unique identifier for this event.
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Gets the UTC timestamp when this event occurred.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Event raised when a vehicle approaches the barrier.
/// </summary>
public sealed record VehicleApproached : DomainEvent;

/// <summary>
/// Event raised when a vehicle completely passes through the barrier.
/// </summary>
public sealed record VehiclePassed : DomainEvent;

/// <summary>
/// Event raised when a vehicle reverses back after passing the barrier.
/// </summary>
public sealed record VehicleReversed : DomainEvent;

/// <summary>
/// Event raised when a card is successfully read by the Mifare reader.
/// </summary>
/// <param name="CardNumber">The card number that was read.</param>
public sealed record CardRead(string CardNumber) : DomainEvent;

/// <summary>
/// Event raised when equipment health status changes.
/// </summary>
/// <param name="EquipmentName">Name of the equipment.</param>
/// <param name="Status">Current health status.</param>
/// <param name="ErrorMessage">Optional error message if status is degraded or critical.</param>
public sealed record EquipmentHealthChanged(
    string EquipmentName,
    HealthStatus Status,
    string? ErrorMessage = null) : DomainEvent;
