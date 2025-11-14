using MediatR;
using ParkingGate.Domain.Models;

namespace ParkingGate.Domain.Queries;

public record GetGateStateQuery : IRequest<GateState>;
