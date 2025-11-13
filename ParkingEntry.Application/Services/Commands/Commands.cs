using Mediator;
using ParkingEntry.Domain.Entities;
using ParkingEntry.Domain.Enums;

namespace ParkingEntry.Application.Services.Commands;

// Barrier Commands

/// <summary>
/// Command to open the barrier.
/// </summary>
public sealed record OpenBarrierCommand : ICommand<BarrierStatus>;

/// <summary>
/// Command to close the barrier.
/// </summary>
public sealed record CloseBarrierCommand : ICommand<BarrierStatus>;

// Light Commands

/// <summary>
/// Command to switch the traffic light color.
/// </summary>
/// <param name="Color">The target light color.</param>
public sealed record SwitchLightCommand(LightColor Color) : ICommand<LightStatus>;

// Mifare Commands

/// <summary>
/// Command to start searching for a card.
/// </summary>
public sealed record StartCardSearchCommand : ICommand<MifareStatus>;

/// <summary>
/// Command to stop searching for a card.
/// </summary>
public sealed record StopCardSearchCommand : ICommand<MifareStatus>;

/// <summary>
/// Command to write to a card.
/// </summary>
/// <param name="Data">The data to write to the card.</param>
public sealed record WriteCardCommand(byte[] Data) : ICommand<MifareStatus>;

/// <summary>
/// Command to stop writing to a card.
/// </summary>
public sealed record StopWriteCardCommand : ICommand<MifareStatus>;

// Access Check Commands

/// <summary>
/// Command to check access rights for a client.
/// </summary>
/// <param name="ClientNumber">The client's card number.</param>
public sealed record CheckAccessCommand(string ClientNumber)
    : ICommand<AccessCheckResult>;

// Debt Calculation Commands

/// <summary>
/// Command to calculate debt for a client.
/// </summary>
/// <param name="ClientNumber">The client's card number.</param>
public sealed record CalculateDebtCommand(string ClientNumber)
    : ICommand<decimal>;
