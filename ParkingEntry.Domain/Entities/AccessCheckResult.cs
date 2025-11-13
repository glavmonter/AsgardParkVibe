namespace ParkingEntry.Domain.Entities;

/// <summary>
/// Represents the result of an access check.
/// </summary>
public sealed class AccessCheckResult
{
    /// <summary>
    /// Gets a value indicating whether access is allowed.
    /// </summary>
    public bool IsAllowed { get; init; }

    /// <summary>
    /// Gets the reason if access is denied.
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    private AccessCheckResult(bool isAllowed, string reason)
    {
        IsAllowed = isAllowed;
        Reason = reason;
    }

    /// <summary>
    /// Creates a result indicating access is allowed.
    /// </summary>
    /// <returns>An allowed access check result.</returns>
    public static AccessCheckResult Allowed() => new(true, string.Empty);

    /// <summary>
    /// Creates a result indicating client was not found.
    /// </summary>
    /// <returns>A denied access check result.</returns>
    public static AccessCheckResult NotFound() => new(false, "Client not found");

    /// <summary>
    /// Creates a result indicating client has no active contract.
    /// </summary>
    /// <returns>A denied access check result.</returns>
    public static AccessCheckResult NoContract() => new(false, "No active contract");

    /// <summary>
    /// Creates a result indicating client is blocked.
    /// </summary>
    /// <returns>A denied access check result.</returns>
    public static AccessCheckResult Blocked() => new(false, "Client is blocked");
}
