using MediatR;
using Microsoft.Extensions.Logging;
using ParkingGate.Domain.Commands;
using ParkingGate.Domain.Events;
using ParkingGate.Infrastructure.Services;

namespace ParkingGate.Application.Handlers.Events;

public class CardReadEventHandler : INotificationHandler<CardReadEvent>
{
    private readonly ILogger<CardReadEventHandler> _logger;
    private readonly IMediator _mediator;
    private readonly IGateStateService _stateService;

    public CardReadEventHandler(
        ILogger<CardReadEventHandler> logger,
        IMediator mediator,
        IGateStateService stateService)
    {
        _logger = logger;
        _mediator = mediator;
        _stateService = stateService;
    }

    public async Task Handle(CardReadEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Card read: {CardNumber} at {Timestamp}", 
            notification.Card.CardNumber, 
            notification.Timestamp);
        
        _stateService.UpdateStateName("CardRead");
        
        // Stop searching
        await _mediator.Send(new StopSearchingCardCommand(), cancellationToken);
        
        // Check access rights (using card number as client number for demo)
        var hasAccess = await _mediator.Send(
            new CheckAccessRightsCommand { ClientNumber = notification.Card.CardNumber }, 
            cancellationToken);
        
        if (hasAccess)
        {
            _logger.LogInformation("Access granted for card {CardNumber}", notification.Card.CardNumber);
            _stateService.UpdateStateName("AccessGranted");
            
            // Open barrier
            await _mediator.Send(new OpenBarrierCommand(), cancellationToken);
        }
        else
        {
            _logger.LogWarning("Access denied for card {CardNumber}", notification.Card.CardNumber);
            _stateService.UpdateStateName("AccessDenied");
        }
    }
}
