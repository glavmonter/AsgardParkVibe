using Mediator;
using Microsoft.Extensions.Logging;
using ParkingEntry.Application.Services.Commands;
using ParkingEntry.Domain.Entities;
using ParkingEntry.Infrastructure.Repositories;

namespace ParkingEntry.Application.Services.Handlers;

/// <summary>
/// Handles access check commands.
/// </summary>
public sealed class AccessCheckHandler :
    ICommandHandler<CheckAccessCommand, AccessCheckResult>
{
    private readonly IClientRepository _clientRepository;
    private readonly ILogger<AccessCheckHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccessCheckHandler"/> class.
    /// </summary>
    /// <param name="clientRepository">The client repository.</param>
    /// <param name="logger">The logger instance.</param>
    public AccessCheckHandler(
        IClientRepository clientRepository,
        ILogger<AccessCheckHandler> logger)
    {
        _clientRepository = clientRepository;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async ValueTask<AccessCheckResult> Handle(
        CheckAccessCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation("Checking access for client {ClientNumber}", command.ClientNumber);

        var client = await _clientRepository
            .GetByNumberAsync(command.ClientNumber, ct);

        if (client is null)
        {
            _logger.LogWarning("Client {ClientNumber} not found", command.ClientNumber);
            return AccessCheckResult.NotFound();
        }

        if (!client.HasActiveContract)
        {
            _logger.LogWarning("Client {ClientNumber} has no active contract", command.ClientNumber);
            return AccessCheckResult.NoContract();
        }

        if (client.IsBlocked)
        {
            _logger.LogWarning("Client {ClientNumber} is blocked", command.ClientNumber);
            return AccessCheckResult.Blocked();
        }

        _logger.LogInformation("Access granted for client {ClientNumber}", command.ClientNumber);
        return AccessCheckResult.Allowed();
    }
}
