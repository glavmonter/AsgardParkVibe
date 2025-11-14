using MediatR;
using ParkingGate.Domain.Models;

namespace ParkingGate.Domain.Commands;

public record StartSearchingCardCommand : IRequest<MifareStatus>;

public record StopSearchingCardCommand : IRequest<MifareStatus>;

public record WriteCardCommand : IRequest<MifareStatus>
{
    public required string CardNumber { get; init; }
    public required string ClientNumber { get; init; }
}

public record StopWritingCardCommand : IRequest<MifareStatus>;
