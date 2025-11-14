using MediatR;
using Microsoft.Extensions.Logging;
using ParkingGate.Domain.Commands;
using ParkingGate.Domain.Events;
using ParkingGate.Domain.Models;
using ParkingGate.Infrastructure.Services;

namespace ParkingGate.Application.Handlers.Events;

public class VehiclePassedThroughEventHandler : INotificationHandler<VehiclePassedThroughEvent>
{
    private readonly ILogger<VehiclePassedThroughEventHandler> _logger;
    private readonly IMediator _mediator;
    private readonly IGateStateService _stateService;

    public VehiclePassedThroughEventHandler(
        ILogger<VehiclePassedThroughEventHandler> logger,
        IMediator mediator,
        IGateStateService stateService)
    {
        _logger = logger;
        _mediator = mediator;
        _stateService = stateService;
    }

    public async Task Handle(VehiclePassedThroughEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Vehicle passed through at {Timestamp}", notification.Timestamp);
        
        _stateService.UpdateStateName("VehiclePassedThrough");
        
        // Close barrier
        await _mediator.Send(new CloseBarrierCommand(), cancellationToken);
        
        // Set traffic light to green
        await _mediator.Send(new SetLightColorCommand { Color = LightStatus.Green }, cancellationToken);
        
        _stateService.UpdateStateName("WaitingForVehicle");
    }
}

public class VehicleBackedOutEventHandler : INotificationHandler<VehicleBackedOutEvent>
{
    private readonly ILogger<VehicleBackedOutEventHandler> _logger;
    private readonly IMediator _mediator;
    private readonly IGateStateService _stateService;

    public VehicleBackedOutEventHandler(
        ILogger<VehicleBackedOutEventHandler> logger,
        IMediator mediator,
        IGateStateService stateService)
    {
        _logger = logger;
        _mediator = mediator;
        _stateService = stateService;
    }

    public async Task Handle(VehicleBackedOutEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Vehicle backed out at {Timestamp}", notification.Timestamp);
        
        _stateService.UpdateStateName("VehicleBackedOut");
        
        // Close barrier
        await _mediator.Send(new CloseBarrierCommand(), cancellationToken);
        
        // Set traffic light to green
        await _mediator.Send(new SetLightColorCommand { Color = LightStatus.Green }, cancellationToken);
        
        _stateService.UpdateStateName("WaitingForVehicle");
    }
}

public class VehicleDepartedEventHandler : INotificationHandler<VehicleDepartedEvent>
{
    private readonly ILogger<VehicleDepartedEventHandler> _logger;
    private readonly IMediator _mediator;
    private readonly IGateStateService _stateService;

    public VehicleDepartedEventHandler(
        ILogger<VehicleDepartedEventHandler> logger,
        IMediator mediator,
        IGateStateService stateService)
    {
        _logger = logger;
        _mediator = mediator;
        _stateService = stateService;
    }

    public async Task Handle(VehicleDepartedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Vehicle departed at {Timestamp}", notification.Timestamp);
        
        _stateService.UpdateStateName("VehicleDeparted");
        
        // Set traffic light to green
        await _mediator.Send(new SetLightColorCommand { Color = LightStatus.Green }, cancellationToken);
        
        _stateService.UpdateStateName("WaitingForVehicle");
    }
}
