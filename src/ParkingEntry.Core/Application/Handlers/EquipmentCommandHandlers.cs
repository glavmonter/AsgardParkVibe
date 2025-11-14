// <copyright file="EquipmentCommandHandlers.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using MediatR;
using ParkingEntry.Core.Application.Commands;
using ParkingEntry.Core.Application.Services;
using ParkingEntry.Core.Domain;

namespace ParkingEntry.Core.Application.Handlers;

/// <summary>
/// Обработчик команды открытия шлагбаума.
/// </summary>
public sealed class OpenBarrierCommandHandler : IRequestHandler<OpenBarrierCommand, BarrierStatus>
{
    private readonly IBarrierService _barrierService;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="OpenBarrierCommandHandler"/>.
    /// </summary>
    public OpenBarrierCommandHandler(IBarrierService barrierService)
    {
        _barrierService = barrierService;
    }

    /// <inheritdoc/>
    public Task<BarrierStatus> Handle(OpenBarrierCommand request, CancellationToken cancellationToken)
    {
        return _barrierService.OpenAsync(cancellationToken);
    }
}

/// <summary>
/// Обработчик команды закрытия шлагбаума.
/// </summary>
public sealed class CloseBarrierCommandHandler : IRequestHandler<CloseBarrierCommand, BarrierStatus>
{
    private readonly IBarrierService _barrierService;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CloseBarrierCommandHandler"/>.
    /// </summary>
    public CloseBarrierCommandHandler(IBarrierService barrierService)
    {
        _barrierService = barrierService;
    }

    /// <inheritdoc/>
    public Task<BarrierStatus> Handle(CloseBarrierCommand request, CancellationToken cancellationToken)
    {
        return _barrierService.CloseAsync(cancellationToken);
    }
}

/// <summary>
/// Обработчик команды переключения светофора.
/// </summary>
public sealed class SetLightCommandHandler : IRequestHandler<SetLightCommand, LightStatus>
{
    private readonly ILightService _lightService;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="SetLightCommandHandler"/>.
    /// </summary>
    public SetLightCommandHandler(ILightService lightService)
    {
        _lightService = lightService;
    }

    /// <inheritdoc/>
    public Task<LightStatus> Handle(SetLightCommand request, CancellationToken cancellationToken)
    {
        return _lightService.SetColorAsync(request.Color, cancellationToken);
    }
}

/// <summary>
/// Обработчик команды начала поиска карты.
/// </summary>
public sealed class StartCardSearchCommandHandler : IRequestHandler<StartCardSearchCommand, MifareStatus>
{
    private readonly IMifareService _mifareService;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="StartCardSearchCommandHandler"/>.
    /// </summary>
    public StartCardSearchCommandHandler(IMifareService mifareService)
    {
        _mifareService = mifareService;
    }

    /// <inheritdoc/>
    public Task<MifareStatus> Handle(StartCardSearchCommand request, CancellationToken cancellationToken)
    {
        return _mifareService.StartSearchAsync(cancellationToken);
    }
}

/// <summary>
/// Обработчик команды остановки поиска карты.
/// </summary>
public sealed class StopCardSearchCommandHandler : IRequestHandler<StopCardSearchCommand, MifareStatus>
{
    private readonly IMifareService _mifareService;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="StopCardSearchCommandHandler"/>.
    /// </summary>
    public StopCardSearchCommandHandler(IMifareService mifareService)
    {
        _mifareService = mifareService;
    }

    /// <inheritdoc/>
    public Task<MifareStatus> Handle(StopCardSearchCommand request, CancellationToken cancellationToken)
    {
        return _mifareService.StopSearchAsync(cancellationToken);
    }
}

/// <summary>
/// Обработчик команды записи карты.
/// </summary>
public sealed class WriteCardCommandHandler : IRequestHandler<WriteCardCommand, MifareStatus>
{
    private readonly IMifareService _mifareService;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="WriteCardCommandHandler"/>.
    /// </summary>
    public WriteCardCommandHandler(IMifareService mifareService)
    {
        _mifareService = mifareService;
    }

    /// <inheritdoc/>
    public Task<MifareStatus> Handle(WriteCardCommand request, CancellationToken cancellationToken)
    {
        return _mifareService.WriteCardAsync(request.CardNumber, cancellationToken);
    }
}

/// <summary>
/// Обработчик команды остановки записи карты.
/// </summary>
public sealed class StopCardWriteCommandHandler : IRequestHandler<StopCardWriteCommand, MifareStatus>
{
    private readonly IMifareService _mifareService;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="StopCardWriteCommandHandler"/>.
    /// </summary>
    public StopCardWriteCommandHandler(IMifareService mifareService)
    {
        _mifareService = mifareService;
    }

    /// <inheritdoc/>
    public Task<MifareStatus> Handle(StopCardWriteCommand request, CancellationToken cancellationToken)
    {
        return _mifareService.StopWriteAsync(cancellationToken);
    }
}
