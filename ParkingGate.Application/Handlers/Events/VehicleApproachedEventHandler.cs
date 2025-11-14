using MediatR;
using Microsoft.Extensions.Logging;
using ParkingGate.Domain.Commands;
using ParkingGate.Domain.Events;
using ParkingGate.Domain.Models;
using ParkingGate.Infrastructure.Services;

namespace ParkingGate.Application.Handlers.Events;

public class VehicleApproachedEventHandler : INotificationHandler<VehicleApproachedEvent>
{
    private readonly ILogger<VehicleApproachedEventHandler> _logger;
    private readonly IMediator _mediator;
    private readonly IGateStateService _stateService;

    public VehicleApproachedEventHandler(
        ILogger<VehicleApproachedEventHandler> logger,
        IMediator mediator,
        IGateStateService stateService)
    {
        _logger = logger;
        _mediator = mediator;
        _stateService = stateService;
    }

    public async Task Handle(VehicleApproachedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Vehicle approached at {Timestamp}", notification.Timestamp);
        
        _stateService.UpdateStateName("VehicleApproached");
        
        // Set traffic light to red
        await _mediator.Send(new SetLightColorCommand { Color = LightStatus.Red }, cancellationToken);
        
        // Start searching for card
        await _mediator.Send(new StartSearchingCardCommand(), cancellationToken);
    }
}
