// <copyright file="EntryCoordinator.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using ParkingEntry.Core.Stateless.Domain;

namespace ParkingEntry.Core.Stateless.Infrastructure;

/// <summary>
/// Координатор событий для state machine.
/// Преобразует внешние события в триггеры Stateless.
/// </summary>
public sealed class EntryCoordinator
{
    private readonly EntryStateMachine _stateMachine;
    private readonly ILogger<EntryCoordinator> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="EntryCoordinator"/>.
    /// </summary>
    /// <param name="stateMachine">State machine.</param>
    /// <param name="logger">Логгер.</param>
    public EntryCoordinator(
        EntryStateMachine stateMachine,
        ILogger<EntryCoordinator> logger)
    {
        _stateMachine = stateMachine;
        _logger = logger;
    }

    /// <summary>
    /// Получить текущее состояние.
    /// </summary>
    public EntryState CurrentState => _stateMachine.CurrentState;

    /// <summary>
    /// Получить текущий номер карты.
    /// </summary>
    public string? CurrentCardNumber => _stateMachine.CurrentCardNumber;

    /// <summary>
    /// Обработать событие "Автомобиль подъехал".
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task HandleVehicleApproachedAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Событие: Автомобиль подъехал");
            await _stateMachine.FireAsync(EntryTrigger.VehicleApproached, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Обработать событие "Карта прочитана".
    /// </summary>
    /// <param name="cardNumber">Номер карты.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task HandleCardReadAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Событие: Карта прочитана - {CardNumber}", cardNumber);
            await _stateMachine.FireAsync(EntryTrigger.CardRead, cardNumber, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Обработать событие "Автомобиль проехал".
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task HandleVehiclePassedAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Событие: Автомобиль проехал");
            await _stateMachine.FireAsync(EntryTrigger.VehiclePassed, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Обработать событие "Автомобиль уехал назад".
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task HandleVehicleReversedAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Событие: Автомобиль уехал назад");
            await _stateMachine.FireAsync(EntryTrigger.VehicleReversed, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Обработать событие "Автомобиль уехал".
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    public async Task HandleVehicleLeftAsync(CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Событие: Автомобиль уехал");
            await _stateMachine.FireAsync(EntryTrigger.VehicleLeft, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    /// <summary>
    /// Сбросить систему.
    /// </summary>
    public void Reset()
    {
        _logger.LogInformation("Сброс системы");
        _stateMachine.Reset();
    }

    /// <summary>
    /// Получить граф состояний.
    /// </summary>
    /// <returns>DOT граф.</returns>
    public string GetStateDiagram()
    {
        return _stateMachine.GetDotGraph();
    }
}
