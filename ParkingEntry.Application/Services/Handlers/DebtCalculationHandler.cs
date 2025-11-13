using Mediator;
using Microsoft.Extensions.Logging;
using ParkingEntry.Application.Services.Commands;
using ParkingEntry.Infrastructure.Repositories;

namespace ParkingEntry.Application.Services.Handlers;

/// <summary>
/// Handles debt calculation commands.
/// </summary>
public sealed class DebtCalculationHandler :
    ICommandHandler<CalculateDebtCommand, decimal>
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<DebtCalculationHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebtCalculationHandler"/> class.
    /// </summary>
    /// <param name="clientRepository">The client repository.</param>
    /// <param name="logger">The logger instance.</param>
    public DebtCalculationHandler(
        IClientRepository clientRepository,
        ILogger<DebtCalculationHandler> logger)
    {
        _clientRepository = clientRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async ValueTask<decimal> Handle(
        CalculateDebtCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation("Calculating debt for client {ClientNumber}", command.ClientNumber);

        var client = await _clientRepository
            .GetByNumberAsync(command.ClientNumber, ct);

        if (client is null)
        {
            _logger.LogWarning("Client {ClientNumber} not found", command.ClientNumber);
            return 0;
        }

        _logger.LogInformation(
            "Client {ClientNumber} has debt of {Debt}",
            command.ClientNumber,
            client.Debt);

        return client.Debt;
    }
}
