namespace ParkingEntry.Application.Infrastructure;

/// <summary>
/// Defines the contract for managing timeouts.
/// </summary>
public interface ITimeoutManager
{
    /// <summary>
    /// Starts a timeout operation.
    /// </summary>
    /// <param name="key">Unique key for the timeout.</param>
    /// <param name="timeout">Duration of the timeout.</param>
    /// <param name="onTimeout">Action to execute when timeout expires.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StartTimeoutAsync(
        string key,
        TimeSpan timeout,
        Func<Task> onTimeout,
        CancellationToken ct);

    /// <summary>
    /// Cancels a timeout operation.
    /// </summary>
    /// <param name="key">Key of the timeout to cancel.</param>
    void CancelTimeout(string key);
}
