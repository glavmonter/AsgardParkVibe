namespace ParkingEntry.Domain.Entities;

/// <summary>
/// Represents a client in the parking system.
/// </summary>
public class Client
{
    /// <summary>
    /// Gets or sets the unique identifier for the client.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the card number associated with the client.
    /// </summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the client has an active contract.
    /// </summary>
    public bool HasActiveContract { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the client is blocked.
    /// </summary>
    public bool IsBlocked { get; set; }

    /// <summary>
    /// Gets or sets the current debt amount for the client.
    /// </summary>
    public decimal Debt { get; set; }

    /// <summary>
    /// Gets or sets the client's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date when the client was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date when the client was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Represents a parking entry session.
/// </summary>
public class EntrySession
{
    /// <summary>
    /// Gets or sets the unique identifier for the session.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the client ID associated with this session.
    /// </summary>
    public Guid ClientId { get; set; }

    /// <summary>
    /// Gets or sets the card number used for entry.
    /// </summary>
    public string CardNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entry timestamp.
    /// </summary>
    public DateTime EntryTime { get; set; }

    /// <summary>
    /// Gets or sets the exit timestamp (null if still in parking).
    /// </summary>
    public DateTime? ExitTime { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether access was granted.
    /// </summary>
    public bool AccessGranted { get; set; }

    /// <summary>
    /// Gets or sets the reason if access was denied.
    /// </summary>
    public string? DenialReason { get; set; }

    /// <summary>
    /// Gets the navigation property to the client.
    /// </summary>
    public Client? Client { get; set; }
}

/// <summary>
/// Represents a card read from the Mifare reader.
/// </summary>
public class Card
{
    /// <summary>
    /// Gets or sets the card number.
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the card was read.
    /// </summary>
    public DateTime ReadAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the card is valid.
    /// </summary>
    public bool IsValid { get; set; }
}
