using Microsoft.Extensions.Logging;
using ParkingEntry.Domain.Enums;
using ParkingEntry.Domain.Events;

namespace ParkingEntry.Infrastructure.Hardware;

/// <summary>
/// Implements Mifare card reader service.
/// </summary>
public sealed class MifareReaderService : IMifareReaderService
{
    private readonly IMifareReader _mifareReader;
    private readonly ILogger<MifareReaderService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MifareReaderService"/> class.
    /// </summary>
    /// <param name="mifareReader">The Mifare reader hardware interface.</param>
    /// <param name="logger">The logger instance.</param>
    public MifareReaderService(
        IMifareReader mifareReader,
        ILogger<MifareReaderService> logger)
    {
        _mifareReader = mifareReader;
        _logger = logger;
        _mifareReader.CardDetected += OnCardDetected;
    }

    /// <inheritdoc/>
    public event EventHandler<CardRead>? CardRead;

    /// <inheritdoc/>
    public async Task<MifareStatus> StartSearchAsync(CancellationToken ct)
    {
        _logger.LogDebug("Starting Mifare card search");
        await _mifareReader.StartAsync(ct);
        return MifareStatus.Searching;
    }

    /// <inheritdoc/>
    public async Task<MifareStatus> StopSearchAsync(CancellationToken ct)
    {
        _logger.LogDebug("Stopping Mifare card search");
        await _mifareReader.StopAsync(ct);
        return MifareStatus.Idle;
    }

    /// <inheritdoc/>
    public Task<MifareStatus> WriteCardAsync(byte[] data, CancellationToken ct)
    {
        _logger.LogDebug("Writing {Length} bytes to card", data.Length);
        // Implementation would interact with hardware
        return Task.FromResult(MifareStatus.Writing);
    }

    /// <inheritdoc/>
    public Task<MifareStatus> StopWriteAsync(CancellationToken ct)
    {
        _logger.LogDebug("Stopping card write operation");
        return Task.FromResult(MifareStatus.Idle);
    }

    private void OnCardDetected(object? sender, CardRead e)
    {
        _logger.LogInformation("Card detected: {CardNumber}", e.CardNumber);
        CardRead?.Invoke(this, e);
    }
}
