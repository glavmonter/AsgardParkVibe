using MediatR;

namespace ParkingGate.Domain.Commands;

public record CheckAccessRightsCommand : IRequest<bool>
{
    public required string ClientNumber { get; init; }
}

public record CalculateDebtCommand : IRequest<decimal>
{
    public required string ClientNumber { get; init; }
}
