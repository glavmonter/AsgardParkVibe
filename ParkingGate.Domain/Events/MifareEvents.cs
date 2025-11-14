using MediatR;
using ParkingGate.Domain.Models;

namespace ParkingGate.Domain.Events;

public record CardReadEvent : INotification
{
    public required MifareCard Card { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public record CardWrittenEvent : INotification
{
    public required string CardNumber { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public record HealthStatusChangedEvent : INotification
{
    public required string ServiceName { get; init; }
    public required HealthStatus Status { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
