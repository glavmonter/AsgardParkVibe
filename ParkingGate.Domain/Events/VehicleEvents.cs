using MediatR;

namespace ParkingGate.Domain.Events;

public record VehicleApproachedEvent : INotification
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public record VehicleDepartedEvent : INotification
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public record VehiclePassedThroughEvent : INotification
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

public record VehicleBackedOutEvent : INotification
{
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
