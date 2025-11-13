namespace ParkingEntry.Domain.Enums;

/// <summary>
/// Represents the state of the entry state machine.
/// </summary>
public enum EntryState
{
    /// <summary>
    /// System is waiting for a vehicle to approach.
    /// </summary>
    Idle,

    /// <summary>
    /// System is reading a card from the Mifare reader.
    /// </summary>
    ReadingCard,

    /// <summary>
    /// System is checking access rights for the client.
    /// </summary>
    CheckingAccess,

    /// <summary>
    /// System is opening the barrier.
    /// </summary>
    OpeningBarrier,

    /// <summary>
    /// System is waiting for the vehicle to pass through.
    /// </summary>
    WaitingPassage,

    /// <summary>
    /// System encountered an error.
    /// </summary>
    Error
}

/// <summary>
/// Represents the status of the barrier.
/// </summary>
public enum BarrierStatus
{
    /// <summary>
    /// Barrier is closed.
    /// </summary>
    Closed,

    /// <summary>
    /// Barrier is open.
    /// </summary>
    Open,

    /// <summary>
    /// Barrier is in the process of opening.
    /// </summary>
    Opening,

    /// <summary>
    /// Barrier is in the process of closing.
    /// </summary>
    Closing,

    /// <summary>
    /// Barrier status is unknown or unavailable.
    /// </summary>
    Unknown
}

/// <summary>
/// Represents light colors for the traffic light.
/// </summary>
public enum LightColor
{
    /// <summary>
    /// No lights are on.
    /// </summary>
    None,

    /// <summary>
    /// Red light is on (stop).
    /// </summary>
    Red,

    /// <summary>
    /// Green light is on (go).
    /// </summary>
    Green,

    /// <summary>
    /// Both red and green lights are on.
    /// </summary>
    Both
}

/// <summary>
/// Represents the status of the traffic light.
/// </summary>
public record struct LightStatus(LightColor CurrentColor);

/// <summary>
/// Represents the status of the Mifare reader.
/// </summary>
public enum MifareStatus
{
    /// <summary>
    /// Reader is idle.
    /// </summary>
    Idle,

    /// <summary>
    /// Reader is searching for a card.
    /// </summary>
    Searching,

    /// <summary>
    /// Reader is writing to a card.
    /// </summary>
    Writing,

    /// <summary>
    /// Reader encountered an error.
    /// </summary>
    Error
}

/// <summary>
/// Represents the health status of equipment.
/// </summary>
public enum HealthStatus
{
    /// <summary>
    /// Equipment is operating normally.
    /// </summary>
    Healthy,

    /// <summary>
    /// Equipment has minor issues but is operational (soft failure).
    /// </summary>
    Degraded,

    /// <summary>
    /// Equipment has critical failure (hard failure).
    /// </summary>
    Critical
}
