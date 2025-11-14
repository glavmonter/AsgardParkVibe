using MediatR;
using ParkingGate.Domain.Commands;
using ParkingGate.Domain.Models;
using ParkingGate.Infrastructure.Hardware;
using ParkingGate.Infrastructure.Services;

namespace ParkingGate.Application.Handlers.Commands;

public class SetLightColorCommandHandler : IRequestHandler<SetLightColorCommand, LightStatus>
{
    private readonly ISlave _slave;
    private readonly IGateStateService _stateService;

    public SetLightColorCommandHandler(ISlave slave, IGateStateService stateService)
    {
        _slave = slave;
        _stateService = stateService;
    }

    public async Task<LightStatus> Handle(SetLightColorCommand request, CancellationToken cancellationToken)
    {
        var status = await _slave.SetLightAsync(request.Color, cancellationToken);
        _stateService.UpdateLightStatus(status);
        return status;
    }
}
