using MediatR;
using ParkingGate.Domain.Commands;
using ParkingGate.Domain.Models;
using ParkingGate.Infrastructure.Hardware;
using ParkingGate.Infrastructure.Services;

namespace ParkingGate.Application.Handlers.Commands;

public class StartSearchingCardCommandHandler : IRequestHandler<StartSearchingCardCommand, MifareStatus>
{
    private readonly IMifareReader _reader;
    private readonly IGateStateService _stateService;

    public StartSearchingCardCommandHandler(IMifareReader reader, IGateStateService stateService)
    {
        _reader = reader;
        _stateService = stateService;
    }

    public async Task<MifareStatus> Handle(StartSearchingCardCommand request, CancellationToken cancellationToken)
    {
        var status = await _reader.StartSearchingAsync(cancellationToken);
        _stateService.UpdateMifareStatus(status);
        return status;
    }
}

public class StopSearchingCardCommandHandler : IRequestHandler<StopSearchingCardCommand, MifareStatus>
{
    private readonly IMifareReader _reader;
    private readonly IGateStateService _stateService;

    public StopSearchingCardCommandHandler(IMifareReader reader, IGateStateService stateService)
    {
        _reader = reader;
        _stateService = stateService;
    }

    public async Task<MifareStatus> Handle(StopSearchingCardCommand request, CancellationToken cancellationToken)
    {
        var status = await _reader.StopSearchingAsync(cancellationToken);
        _stateService.UpdateMifareStatus(status);
        return status;
    }
}

public class WriteCardCommandHandler : IRequestHandler<WriteCardCommand, MifareStatus>
{
    private readonly IMifareReader _reader;
    private readonly IGateStateService _stateService;

    public WriteCardCommandHandler(IMifareReader reader, IGateStateService stateService)
    {
        _reader = reader;
        _stateService = stateService;
    }

    public async Task<MifareStatus> Handle(WriteCardCommand request, CancellationToken cancellationToken)
    {
        var status = await _reader.StartWritingAsync(request.CardNumber, request.ClientNumber, cancellationToken);
        _stateService.UpdateMifareStatus(status);
        return status;
    }
}

public class StopWritingCardCommandHandler : IRequestHandler<StopWritingCardCommand, MifareStatus>
{
    private readonly IMifareReader _reader;
    private readonly IGateStateService _stateService;

    public StopWritingCardCommandHandler(IMifareReader reader, IGateStateService stateService)
    {
        _reader = reader;
        _stateService = stateService;
    }

    public async Task<MifareStatus> Handle(StopWritingCardCommand request, CancellationToken cancellationToken)
    {
        var status = await _reader.StopWritingAsync(cancellationToken);
        _stateService.UpdateMifareStatus(status);
        return status;
    }
}
