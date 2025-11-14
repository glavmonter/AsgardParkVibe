using MediatR;
using ParkingGate.Domain.Models;

namespace ParkingGate.Domain.Commands;

public record OpenBarrierCommand : IRequest<BarrierStatus>;

public record CloseBarrierCommand : IRequest<BarrierStatus>;
