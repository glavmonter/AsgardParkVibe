// <copyright file="ITimeoutManager.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

namespace ParkingEntry.Core.Application.Services;

/// <summary>
/// Менеджер таймаутов для операций.
/// </summary>
public interface ITimeoutManager
{
    /// <summary>
    /// Запустить таймер с таймаутом.
    /// </summary>
    /// <param name="operationName">Название операции.</param>
    /// <param name="timeout">Время таймаута.</param>
    /// <param name="onTimeout">Действие при таймауте.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>ID таймера.</returns>
    Task<Guid> StartTimerAsync(
        string operationName,
        TimeSpan timeout,
        Func<Task> onTimeout,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Отменить таймер.
    /// </summary>
    /// <param name="timerId">ID таймера.</param>
    void CancelTimer(Guid timerId);

    /// <summary>
    /// Проверить, активен ли таймер.
    /// </summary>
    /// <param name="timerId">ID таймера.</param>
    /// <returns>True, если таймер активен.</returns>
    bool IsTimerActive(Guid timerId);
}
