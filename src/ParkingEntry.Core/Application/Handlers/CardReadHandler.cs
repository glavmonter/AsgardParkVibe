// <copyright file="CardReadHandler.cs" company="RPS">
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
/// Обработчик события "Карта прочитана".
/// </summary>
public sealed class CardReadHandler : INotificationHandler<CardRead>
{
    private readonly IStateContext _stateContext;
    private readonly IMediator _mediator;
    private readonly ILogger<CardReadHandler> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CardReadHandler"/>.
    /// </summary>
    /// <param name="stateContext">Контекст состояния.</param>
    /// <param name="mediator">Медиатор.</param>
    /// <param name="logger">Логгер.</param>
    public CardReadHandler(
        IStateContext stateContext,
        IMediator mediator,
        ILogger<CardReadHandler> logger)
    {
        _stateContext = stateContext;
        _mediator = mediator;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task Handle(CardRead notification, CancellationToken cancellationToken)
    {
        if (_stateContext.CurrentState != EntryState.ReadingCard)
        {
            _logger.LogWarning(
                "Игнорируем CardRead в состоянии {State}",
                _stateContext.CurrentState);
            return;
        }

        _logger.LogInformation("Карта прочитана: {CardNumber}", notification.CardNumber);

        // Сохраняем номер карты
        _stateContext.SetCardNumber(notification.CardNumber);

        // Останавливаем поиск карты
        await _mediator.Send(new StopCardSearchCommand(), cancellationToken);

        // Переходим к проверке доступа
        _stateContext.SetState(EntryState.CheckingAccess);

        // Запускаем проверку доступа
        var accessResult = await _mediator.Send(
            new CheckAccessCommand(notification.CardNumber),
            cancellationToken);

        if (accessResult.IsAllowed)
        {
            _logger.LogInformation("Доступ разрешен для {CardNumber}", notification.CardNumber);
            _stateContext.SetState(EntryState.OpeningBarrier);
            await _mediator.Send(new OpenBarrierCommand(), cancellationToken);
            _stateContext.SetState(EntryState.WaitingPassage);
        }
        else
        {
            _logger.LogWarning(
                "Доступ запрещен для {CardNumber}. Причина: {Reason}",
                notification.CardNumber,
                accessResult.Reason);
            _stateContext.SetState(EntryState.AccessDenied);
        }
    }
}
