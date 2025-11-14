// <copyright file="TimeoutManager.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ParkingEntry.Core.Application.Services;

namespace ParkingEntry.Core.Infrastructure;

/// <summary>
/// Менеджер таймаутов для операций.
/// </summary>
public sealed class TimeoutManager : ITimeoutManager
{
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _timers = new();
    private readonly ILogger<TimeoutManager> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="TimeoutManager"/>.
    /// </summary>
    public TimeoutManager(ILogger<TimeoutManager> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<Guid> StartTimerAsync(
        string operationName,
        TimeSpan timeout,
        Func<Task> onTimeout,
        CancellationToken cancellationToken = default)
    {
        var timerId = Guid.NewGuid();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _timers[timerId] = cts;

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(timeout, cts.Token);

                if (!cts.Token.IsCancellationRequested)
                {
                    _logger.LogWarning("Таймаут операции {Operation} (ID: {TimerId})", operationName, timerId);
                    await onTimeout();
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug("Таймер {TimerId} отменен", timerId);
            }
            finally
            {
                _timers.TryRemove(timerId, out _);
                cts.Dispose();
            }
        }, CancellationToken.None);

        return timerId;
    }

    /// <inheritdoc/>
    public void CancelTimer(Guid timerId)
    {
        if (_timers.TryRemove(timerId, out var cts))
        {
            cts.Cancel();
            cts.Dispose();
        }
    }

    /// <inheritdoc/>
    public bool IsTimerActive(Guid timerId)
    {
        return _timers.ContainsKey(timerId);
    }
}
