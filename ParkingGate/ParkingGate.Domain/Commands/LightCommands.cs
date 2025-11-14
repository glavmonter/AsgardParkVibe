using MediatR;
using ParkingGate.Domain.Models;

namespace ParkingGate.Domain.Commands;

public record SetLightColorCommand : IRequest<LightStatus>
{
    public required LightStatus Color { get; init; }
}
