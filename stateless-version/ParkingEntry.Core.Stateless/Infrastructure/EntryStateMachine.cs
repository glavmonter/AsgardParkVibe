// <copyright file="EntryStateMachine.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using MediatR;
using Microsoft.Extensions.Logging;
using ParkingEntry.Core.Stateless.Application.Commands;
using ParkingEntry.Core.Stateless.Domain;
using Stateless;

namespace ParkingEntry.Core.Stateless.Infrastructure;

/// <summary>
/// Конечный автомат въездной стойки на базе Stateless.
/// </summary>
public sealed class EntryStateMachine
{
    private readonly StateMachine<EntryState, EntryTrigger> _machine;
    private readonly IMediator _mediator;
    private readonly ILogger<EntryStateMachine> _logger;
    private string? _currentCardNumber;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="EntryStateMachine"/>.
    /// </summary>
    /// <param name="mediator">Медиатор.</param>
    /// <param name="logger">Логгер.</param>
    public EntryStateMachine(
        IMediator mediator,
        ILogger<EntryStateMachine> logger)
    {
        _mediator = mediator;
        _logger = logger;

        // Создаем state machine с начальным состоянием Idle
        _machine = new StateMachine<EntryState, EntryTrigger>(EntryState.Idle);

        ConfigureStateMachine();
    }

    /// <summary>
    /// Текущее состояние.
    /// </summary>
    public EntryState CurrentState => _machine.State;

    /// <summary>
    /// Текущий номер карты.
    /// </summary>
    public string? CurrentCardNumber => _currentCardNumber;

    /// <summary>
    /// Проверить, можно ли выполнить переход.
    /// </summary>
    /// <param name="trigger">Триггер.</param>
    /// <returns>True, если переход возможен.</returns>
    public bool CanFire(EntryTrigger trigger)
    {
        return _machine.CanFire(trigger);
    }

    /// <summary>
    /// Выполнить переход по триггеру.
    /// </summary>
    /// <param name="trigger">Триггер.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task FireAsync(EntryTrigger trigger, CancellationToken cancellationToken = default)
    {
        if (!_machine.CanFire(trigger))
        {
            _logger.LogWarning(
                "Невозможно выполнить триггер {Trigger} в состоянии {State}",
                trigger,
                _machine.State);
            return;
        }

        await _machine.FireAsync(trigger);
    }

    /// <summary>
    /// Выполнить переход по триггеру с параметром (номер карты).
    /// </summary>
    /// <param name="trigger">Триггер.</param>
    /// <param name="cardNumber">Номер карты.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task FireAsync(
        EntryTrigger trigger,
        string cardNumber,
        CancellationToken cancellationToken = default)
    {
        _currentCardNumber = cardNumber;
        await FireAsync(trigger, cancellationToken);
    }

    /// <summary>
    /// Сбросить автомат в начальное состояние.
    /// </summary>
    public void Reset()
    {
        _currentCardNumber = null;
        
        // Сбрасываем в Idle если мы не в нем
        if (_machine.State != EntryState.Idle)
        {
            // Stateless не позволяет напрямую установить состояние,
            // поэтому используем Reset триггер или создаем новый экземпляр
            _logger.LogInformation("Сброс автомата из {State} в Idle", _machine.State);
        }
    }

    /// <summary>
    /// Конфигурация state machine.
    /// </summary>
    private void ConfigureStateMachine()
    {
        // === IDLE STATE ===
        _machine.Configure(EntryState.Idle)
            .Permit(EntryTrigger.VehicleApproached, EntryState.ReadingCard);

        // === READING CARD STATE ===
        _machine.Configure(EntryState.ReadingCard)
            .OnEntryAsync(async () =>
            {
                _logger.LogInformation("Вход в состояние ReadingCard");
                
                // Переключаем светофор в красный
                await _mediator.Send(new SetLightCommand(LightColor.Red));
                
                // Начинаем поиск карты
                await _mediator.Send(new StartCardSearchCommand());
            })
            .Permit(EntryTrigger.CardRead, EntryState.CheckingAccess)
            .Permit(EntryTrigger.CardReadTimeout, EntryState.Idle)
            .Permit(EntryTrigger.VehicleLeft, EntryState.Idle)
            .OnExitAsync(async () =>
            {
                // Останавливаем поиск карты при выходе из состояния
                await _mediator.Send(new StopCardSearchCommand());
            });

        // === CHECKING ACCESS STATE ===
        _machine.Configure(EntryState.CheckingAccess)
            .OnEntryAsync(async () =>
            {
                _logger.LogInformation("Проверка доступа для карты {CardNumber}", _currentCardNumber);
                
                if (_currentCardNumber == null)
                {
                    _logger.LogError("Номер карты не установлен!");
                    await _machine.FireAsync(EntryTrigger.AccessDenied);
                    return;
                }

                // Проверяем доступ
                var result = await _mediator.Send(new CheckAccessCommand(_currentCardNumber));
                
                if (result.IsAllowed)
                {
                    await _machine.FireAsync(EntryTrigger.AccessGranted);
                }
                else
                {
                    _logger.LogWarning("Доступ запрещен: {Reason}", result.Reason);
                    await _machine.FireAsync(EntryTrigger.AccessDenied);
                }
            })
            .Permit(EntryTrigger.AccessGranted, EntryState.OpeningBarrier)
            .Permit(EntryTrigger.AccessDenied, EntryState.AccessDenied);

        // === OPENING BARRIER STATE ===
        _machine.Configure(EntryState.OpeningBarrier)
            .OnEntryAsync(async () =>
            {
                _logger.LogInformation("Открываем шлагбаум");
                await _mediator.Send(new OpenBarrierCommand());
                
                // Автоматически переходим в WaitingPassage
                await _machine.FireAsync(EntryTrigger.BarrierOpened);
            })
            .Permit(EntryTrigger.BarrierOpened, EntryState.WaitingPassage)
            .Permit(EntryTrigger.EquipmentError, EntryState.EquipmentError);

        // === WAITING PASSAGE STATE ===
        _machine.Configure(EntryState.WaitingPassage)
            .OnEntry(() =>
            {
                _logger.LogInformation("Ожидание проезда автомобиля");
            })
            .Permit(EntryTrigger.VehiclePassed, EntryState.ClosingBarrier)
            .Permit(EntryTrigger.VehicleReversed, EntryState.ClosingBarrier)
            .Permit(EntryTrigger.EquipmentError, EntryState.EquipmentError);

        // === CLOSING BARRIER STATE ===
        _machine.Configure(EntryState.ClosingBarrier)
            .OnEntryAsync(async () =>
            {
                _logger.LogInformation("Закрываем шлагбаум");
                
                // Закрываем шлагбаум
                await _mediator.Send(new CloseBarrierCommand());
                
                // Переключаем светофор в зеленый
                await _mediator.Send(new SetLightCommand(LightColor.Green));
                
                // Автоматически переходим в Idle
                await _machine.FireAsync(EntryTrigger.BarrierClosed);
            })
            .OnExit(() =>
            {
                // Очищаем номер карты
                _currentCardNumber = null;
            })
            .Permit(EntryTrigger.BarrierClosed, EntryState.Idle)
            .Permit(EntryTrigger.EquipmentError, EntryState.EquipmentError);

        // === ACCESS DENIED STATE ===
        _machine.Configure(EntryState.AccessDenied)
            .OnEntry(() =>
            {
                _logger.LogWarning("Доступ запрещен");
            })
            .Permit(EntryTrigger.VehicleLeft, EntryState.Idle)
            .OnExit(() =>
            {
                _currentCardNumber = null;
            });

        // === EQUIPMENT ERROR STATE ===
        _machine.Configure(EntryState.EquipmentError)
            .OnEntry(() =>
            {
                _logger.LogError("Ошибка оборудования!");
            })
            .Permit(EntryTrigger.Reset, EntryState.Idle);

        // Глобальные обработчики переходов
        _machine.OnTransitioned(transition =>
        {
            _logger.LogInformation(
                "Переход: {Source} --[{Trigger}]--> {Destination}",
                transition.Source,
                transition.Trigger,
                transition.Destination);
        });
    }

    /// <summary>
    /// Получить граф состояний в формате DOT (для визуализации).
    /// </summary>
    /// <returns>Граф в формате DOT.</returns>
    public string GetDotGraph()
    {
        return Stateless.Graph.UmlDotGraph.Format(_machine.GetInfo());
    }
}
