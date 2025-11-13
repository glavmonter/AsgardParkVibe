using ParkingEntry.Domain.Enums;
using ParkingEntry.Domain.Events;

namespace ParkingEntry.Infrastructure.Hardware;

/// <summary>
/// Defines the contract for barrier control service.
/// </summary>
public interface IBarrierService
{
    /// <summary>
    /// Opens the barrier.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The current barrier status.</returns>
    Task<BarrierStatus> OpenAsync(CancellationToken ct);

    /// <summary>
    /// Closes the barrier.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The current barrier status.</returns>
    Task<BarrierStatus> CloseAsync(CancellationToken ct);
}

/// <summary>
/// Defines the contract for traffic light control service.
/// </summary>
public interface ILightService
{
    /// <summary>
    /// Switches the traffic light to the specified color.
    /// </summary>
    /// <param name="color">The target light color.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The current light status.</returns>
    Task<LightStatus> SwitchAsync(LightColor color, CancellationToken ct);
}

/// <summary>
/// Defines the contract for Mifare card reader service.
/// </summary>
public interface IMifareReaderService
{
    /// <summary>
    /// Starts searching for a card.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The current Mifare reader status.</returns>
    Task<MifareStatus> StartSearchAsync(CancellationToken ct);

    /// <summary>
    /// Stops searching for a card.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The current Mifare reader status.</returns>
    Task<MifareStatus> StopSearchAsync(CancellationToken ct);

    /// <summary>
    /// Writes data to a card.
    /// </summary>
    /// <param name="data">The data to write.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The current Mifare reader status.</returns>
    Task<MifareStatus> WriteCardAsync(byte[] data, CancellationToken ct);

    /// <summary>
    /// Stops writing to a card.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The current Mifare reader status.</returns>
    Task<MifareStatus> StopWriteAsync(CancellationToken ct);

    /// <summary>
    /// Event raised when a card is read.
    /// </summary>
    event EventHandler<CardRead>? CardRead;
}

/// <summary>
/// Defines the contract for the slave worker that communicates with hardware.
/// </summary>
public interface ISlaveWorker
{
    /// <summary>
    /// Sends a command to the hardware.
    /// </summary>
    /// <param name="command">The command to send.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendCommandAsync(string command, CancellationToken ct);

    /// <summary>
    /// Event raised when a vehicle approaches the barrier.
    /// </summary>
    event EventHandler? VehicleApproached;

    /// <summary>
    /// Event raised when a vehicle passes through the barrier.
    /// </summary>
    event EventHandler? VehiclePassed;

    /// <summary>
    /// Event raised when a vehicle reverses back.
    /// </summary>
    event EventHandler? VehicleReversed;

    /// <summary>
    /// Event raised when equipment health status changes.
    /// </summary>
    event EventHandler<EquipmentHealthChanged>? HealthStatusChanged;
}

/// <summary>
/// Defines the contract for the Mifare reader hardware interface.
/// </summary>
public interface IMifareReader
{
    /// <summary>
    /// Starts the card reader.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StartAsync(CancellationToken ct);

    /// <summary>
    /// Stops the card reader.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StopAsync(CancellationToken ct);

    /// <summary>
    /// Event raised when a card is detected and read.
    /// </summary>
    event EventHandler<CardRead>? CardDetected;

    /// <summary>
    /// Event raised when reader health status changes.
    /// </summary>
    event EventHandler<EquipmentHealthChanged>? HealthStatusChanged;
}
