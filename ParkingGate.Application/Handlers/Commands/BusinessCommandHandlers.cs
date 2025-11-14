using MediatR;
using Microsoft.Extensions.Logging;
using ParkingGate.Domain.Commands;

namespace ParkingGate.Application.Handlers.Commands;

public class CheckAccessRightsCommandHandler : IRequestHandler<CheckAccessRightsCommand, bool>
{
    private readonly ILogger<CheckAccessRightsCommandHandler> _logger;

    public CheckAccessRightsCommandHandler(ILogger<CheckAccessRightsCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task<bool> Handle(CheckAccessRightsCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking access rights for client {ClientNumber}", request.ClientNumber);
        
        // TODO: Implement database check
        // For now, simulate access check - allow if client number starts with "1"
        var hasAccess = request.ClientNumber.StartsWith("1");
        
        _logger.LogInformation("Client {ClientNumber} access: {HasAccess}", request.ClientNumber, hasAccess);
        return Task.FromResult(hasAccess);
    }
}

public class CalculateDebtCommandHandler : IRequestHandler<CalculateDebtCommand, decimal>
{
    private readonly ILogger<CalculateDebtCommandHandler> _logger;

    public CalculateDebtCommandHandler(ILogger<CalculateDebtCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task<decimal> Handle(CalculateDebtCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Calculating debt for client {ClientNumber}", request.ClientNumber);
        
        // TODO: Implement database calculation
        // For now, simulate debt calculation
        var debt = Random.Shared.Next(0, 1000);
        
        _logger.LogInformation("Client {ClientNumber} debt: {Debt}", request.ClientNumber, debt);
        return Task.FromResult((decimal)debt);
    }
}
