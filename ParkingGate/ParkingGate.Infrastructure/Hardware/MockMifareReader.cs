using MediatR;
using Microsoft.Extensions.Logging;
using ParkingGate.Domain.Events;
using ParkingGate.Domain.Models;

namespace ParkingGate.Infrastructure.Hardware;

public class MockMifareReader : IMifareReader
{
    private readonly ILogger<MockMifareReader> _logger;
    private readonly IPublisher _publisher;
    private MifareStatus _currentStatus = MifareStatus.Idle;
    private CancellationTokenSource? _searchCts;

    public MockMifareReader(ILogger<MockMifareReader> logger, IPublisher publisher)
    {
        _logger = logger;
        _publisher = publisher;
    }

    public async Task<MifareStatus> StartSearchingAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting card search");
        _currentStatus = MifareStatus.Searching;
        
        _searchCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        // Simulate card detection after random delay
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(Random.Shared.Next(2000, 5000), _searchCts.Token);
                
                var card = new MifareCard($"CARD-{Random.Shared.Next(1000, 9999)}", DateTime.UtcNow);
                _currentStatus = MifareStatus.CardRead;
                
                await _publisher.Publish(new CardReadEvent { Card = card }, _searchCts.Token);
                _logger.LogInformation("Card read: {CardNumber}", card.CardNumber);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Card search cancelled");
            }
        }, cancellationToken);

        return _currentStatus;
    }

    public Task<MifareStatus> StopSearchingAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping card search");
        _searchCts?.Cancel();
        _currentStatus = MifareStatus.Idle;
        return Task.FromResult(_currentStatus);
    }

    public async Task<MifareStatus> StartWritingAsync(string cardNumber, string clientNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Writing card {CardNumber} for client {ClientNumber}", cardNumber, clientNumber);
        _currentStatus = MifareStatus.Writing;
        
        await Task.Delay(1000, cancellationToken);
        
        _currentStatus = MifareStatus.CardWritten;
        await _publisher.Publish(new CardWrittenEvent { CardNumber = cardNumber }, cancellationToken);
        
        return _currentStatus;
    }

    public Task<MifareStatus> StopWritingAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping card writing");
        _currentStatus = MifareStatus.Idle;
        return Task.FromResult(_currentStatus);
    }

    public Task<HealthStatus> GetHealthStatusAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(HealthStatus.Healthy);
    }
}
