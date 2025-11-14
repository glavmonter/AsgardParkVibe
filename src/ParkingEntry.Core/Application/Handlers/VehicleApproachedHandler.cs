// <copyright file="VehicleApproachedHandler.cs" company="RPS">
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
/// Обработчик события "Автомобиль подъехал к шлагбауму".
/// </summary>
public sealed class VehicleApproachedHandler : INotificationHandler<VehicleApproached>
{
    private readonly IStateContext _stateContext;
    private readonly IMediator _mediator;
    private readonly ILogger<VehicleApproachedHandler> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="VehicleApproachedHandler"/>.
    /// </summary>
    /// <param name="stateContext">Контекст состояния.</param>
    /// <param name="mediator">Медиатор.</param>
    /// <param name="logger">Логгер.</param>
    public VehicleApproachedHandler(
        IStateContext stateContext,
        IMediator mediator,
        ILogger<VehicleApproachedHandler> logger)
    {
        _stateContext = stateContext;
        _mediator = mediator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task Handle(VehicleApproached notification, CancellationToken cancellationToken)
    {
        if (_stateContext.CurrentState != EntryState.Idle)
        {
            _logger.LogWarning(
                "Игнорируем VehicleApproached в состоянии {State}",
                _stateContext.CurrentState);
            return;
        }

        _logger.LogInformation("Автомобиль подъехал. Переход в состояние ReadingCard");

        // Переключаем светофор в красный
        await _mediator.Send(new SetLightCommand(LightColor.Red), cancellationToken);

        // Начинаем поиск карты
        await _mediator.Send(new StartCardSearchCommand(), cancellationToken);

        // Меняем состояние
        _stateContext.SetState(EntryState.ReadingCard);
    }
}
