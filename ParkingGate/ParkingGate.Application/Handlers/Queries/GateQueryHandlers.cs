using MediatR;
using ParkingGate.Domain.Models;
using ParkingGate.Domain.Queries;
using ParkingGate.Infrastructure.Services;

namespace ParkingGate.Application.Handlers.Queries;

public class GetGateStateQueryHandler : IRequestHandler<GetGateStateQuery, GateState>
{
    private readonly IGateStateService _stateService;

    public GetGateStateQueryHandler(IGateStateService stateService)
    {
        _stateService = stateService;
    }

    public Task<GateState> Handle(GetGateStateQuery request, CancellationToken cancellationToken)
    {
        return Task.FromResult(_stateService.GetCurrentState());
    }
}
