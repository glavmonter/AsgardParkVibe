using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace ParkingEntry.Application.Infrastructure;

/// <summary>
/// Manages timeout operations with cancellation support.
/// </summary>
public sealed class TimeoutManager : ITimeoutManager
{
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _timeouts = new();
    private readonly ILogger<TimeoutManager> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TimeoutManager"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public TimeoutManager(ILogger<TimeoutManager> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task StartTimeoutAsync(
        string key,
        TimeSpan timeout,
        Func<Task> onTimeout,
        CancellationToken ct)
    {
        CancelTimeout(key);

        var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        _timeouts[key] = cts;

        _logger.LogDebug("Starting timeout {Key} for {Timeout}", key, timeout);

        try
        {
            await Task.Delay(timeout, cts.Token);

            if (!cts.Token.IsCancellationRequested)
            {
                _logger.LogWarning("Timeout {Key} expired after {Timeout}", key, timeout);
                await onTimeout();
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogDebug("Timeout {Key} was cancelled", key);
        }
        finally
        {
            _timeouts.TryRemove(key, out _);
            cts.Dispose();
        }
    }

    /// <inheritdoc/>
    public void CancelTimeout(string key)
    {
        if (_timeouts.TryRemove(key, out var cts))
        {
            _logger.LogDebug("Cancelling timeout {Key}", key);
            cts.Cancel();
            cts.Dispose();
        }
    }
}
