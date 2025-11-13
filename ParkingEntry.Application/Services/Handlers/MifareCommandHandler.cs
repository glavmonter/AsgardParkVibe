using Mediator;
using Microsoft.Extensions.Logging;
using ParkingEntry.Application.Services.Commands;
using ParkingEntry.Domain.Enums;
using ParkingEntry.Infrastructure.Hardware;

namespace ParkingEntry.Application.Services.Handlers;

/// <summary>
/// Handles Mifare reader commands.
/// </summary>
public sealed class MifareCommandHandler :
    ICommandHandler<StartCardSearchCommand, MifareStatus>,
    ICommandHandler<StopCardSearchCommand, MifareStatus>,
    ICommandHandler<WriteCardCommand, MifareStatus>,
    ICommandHandler<StopWriteCardCommand, MifareStatus>
{
    private readonly IMifareReaderService _mifareReaderService;
    private readonly ILogger<MifareCommandHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MifareCommandHandler"/> class.
    /// </summary>
    /// <param name="mifareReaderService">The Mifare reader service.</param>
    /// <param name="logger">The logger instance.</param>
    public MifareCommandHandler(
        IMifareReaderService mifareReaderService,
        ILogger<MifareCommandHandler> logger)
    {
        _mifareReaderService = mifareReaderService;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async ValueTask<MifareStatus> Handle(
        StartCardSearchCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation("Starting card search");
        return await _mifareReaderService.StartSearchAsync(ct);
    }

    /// <inheritdoc/>
    public async ValueTask<MifareStatus> Handle(
        StopCardSearchCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation("Stopping card search");
        return await _mifareReaderService.StopSearchAsync(ct);
    }

    /// <inheritdoc/>
    public async ValueTask<MifareStatus> Handle(
        WriteCardCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation("Writing to card");
        return await _mifareReaderService.WriteCardAsync(command.Data, ct);
    }

    /// <inheritdoc/>
    public async ValueTask<MifareStatus> Handle(
        StopWriteCardCommand command,
        CancellationToken ct)
    {
        _logger.LogInformation("Stopping card write");
        return await _mifareReaderService.StopWriteAsync(ct);
    }
}
