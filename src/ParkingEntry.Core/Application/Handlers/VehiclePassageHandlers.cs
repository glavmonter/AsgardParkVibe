// <copyright file="VehiclePassageHandlers.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using MediatR;
using Microsoft.Extensions.Logging;
using ParkingEntry.Core.Application.Commands;
using ParkingEntry.Core.Application.StateMachine;
using ParkingEntry.Core.Domain;
using ParkingEntry.Core.Domain.Events;

namespace ParkingEntry.Core.Application.Handlers;

/// <summary>
/// Обработчик события "Автомобиль проехал через шлагбаум".
/// </summary>
public sealed class VehiclePassedHandler : INotificationHandler<VehiclePassed>
{
    private readonly IStateContext _stateContext;
    private readonly IMediator _mediator;
    private readonly ILogger<VehiclePassedHandler> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="VehiclePassedHandler"/>.
    /// </summary>
    public VehiclePassedHandler(
        IStateContext stateContext,
        IMediator mediator,
        ILogger<VehiclePassedHandler> logger)
    {
        _stateContext = stateContext;
        _mediator = mediator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task Handle(VehiclePassed notification, CancellationToken cancellationToken)
    {
        if (_stateContext.CurrentState != EntryState.WaitingPassage)
        {
            _logger.LogWarning(
                "Игнорируем VehiclePassed в состоянии {State}",
                _stateContext.CurrentState);
            return;
        }

        _logger.LogInformation("Автомобиль проехал полностью");

        // Закрываем шлагбаум
        _stateContext.SetState(EntryState.ClosingBarrier);
        await _mediator.Send(new CloseBarrierCommand(), cancellationToken);

        // Переключаем светофор в зеленый
        await _mediator.Send(new SetLightCommand(LightColor.Green), cancellationToken);

        // Возвращаемся в начальное состояние
        _stateContext.Reset();
    }
}

/// <summary>
/// Обработчик события "Автомобиль уехал назад".
/// </summary>
public sealed class VehicleReversedHandler : INotificationHandler<VehicleReversed>
{
    private readonly IStateContext _stateContext;
    private readonly IMediator _mediator;
    private readonly ILogger<VehicleReversedHandler> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="VehicleReversedHandler"/>.
    /// </summary>
    public VehicleReversedHandler(
        IStateContext stateContext,
        IMediator mediator,
        ILogger<VehicleReversedHandler> logger)
    {
        _stateContext = stateContext;
        _mediator = mediator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task Handle(VehicleReversed notification, CancellationToken cancellationToken)
    {
        if (_stateContext.CurrentState != EntryState.WaitingPassage)
        {
            _logger.LogWarning(
                "Игнорируем VehicleReversed в состоянии {State}",
                _stateContext.CurrentState);
            return;
        }

        _logger.LogInformation("Автомобиль уехал назад");

        // Закрываем шлагбаум
        _stateContext.SetState(EntryState.ClosingBarrier);
        await _mediator.Send(new CloseBarrierCommand(), cancellationToken);

        // Переключаем светофор в зеленый
        await _mediator.Send(new SetLightCommand(LightColor.Green), cancellationToken);

        // Возвращаемся в начальное состояние
        _stateContext.Reset();
    }
}

/// <summary>
/// Обработчик события "Автомобиль уехал от шлагбаума".
/// </summary>
public sealed class VehicleLeftHandler : INotificationHandler<VehicleLeft>
{
    private readonly IStateContext _stateContext;
    private readonly IMediator _mediator;
    private readonly ILogger<VehicleLeftHandler> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="VehicleLeftHandler"/>.
    /// </summary>
    public VehicleLeftHandler(
        IStateContext stateContext,
        IMediator mediator,
        ILogger<VehicleLeftHandler> logger)
    {
        _stateContext = stateContext;
        _mediator = mediator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task Handle(VehicleLeft notification, CancellationToken cancellationToken)
    {
        // Может быть вызван в состояниях ReadingCard или AccessDenied
        if (_stateContext.CurrentState != EntryState.ReadingCard &&
            _stateContext.CurrentState != EntryState.AccessDenied)
        {
            _logger.LogWarning(
                "Игнорируем VehicleLeft в состоянии {State}",
                _stateContext.CurrentState);
            return;
        }

        _logger.LogInformation("Автомобиль уехал от шлагбаума");

        // Останавливаем поиск карты если он был активен
        if (_stateContext.CurrentState == EntryState.ReadingCard)
        {
            await _mediator.Send(new StopCardSearchCommand(), cancellationToken);
        }

        // Переключаем светофор в зеленый
        await _mediator.Send(new SetLightCommand(LightColor.Green), cancellationToken);

        // Возвращаемся в начальное состояние
        _stateContext.Reset();
    }
}
