using MediatR;
using ParkingGate.Domain.Commands;
using ParkingGate.Domain.Models;
using ParkingGate.Infrastructure.Hardware;
using ParkingGate.Infrastructure.Services;

namespace ParkingGate.Application.Handlers.Commands;

public class OpenBarrierCommandHandler : IRequestHandler<OpenBarrierCommand, BarrierStatus>
{
    private readonly ISlave _slave;
    private readonly IGateStateService _stateService;

    public OpenBarrierCommandHandler(ISlave slave, IGateStateService stateService)
    {
        _slave = slave;
        _stateService = stateService;
    }

    public async Task<BarrierStatus> Handle(OpenBarrierCommand request, CancellationToken cancellationToken)
    {
        var status = await _slave.OpenBarrierAsync(cancellationToken);
        _stateService.UpdateBarrierStatus(status);
        return status;
    }
}

public class CloseBarrierCommandHandler : IRequestHandler<CloseBarrierCommand, BarrierStatus>
{
    private readonly ISlave _slave;
    private readonly IGateStateService _stateService;

    public CloseBarrierCommandHandler(ISlave slave, IGateStateService stateService)
    {
        _slave = slave;
        _stateService = stateService;
    }

    public async Task<BarrierStatus> Handle(CloseBarrierCommand request, CancellationToken cancellationToken)
    {
        var status = await _slave.CloseBarrierAsync(cancellationToken);
        _stateService.UpdateBarrierStatus(status);
        return status;
    }
}
