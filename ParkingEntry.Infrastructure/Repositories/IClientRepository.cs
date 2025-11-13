using ParkingEntry.Domain.Entities;

namespace ParkingEntry.Infrastructure.Repositories;

/// <summary>
/// Defines the contract for client repository.
/// </summary>
public interface IClientRepository
{
    /// <summary>
    /// Gets a client by card number.
    /// </summary>
    /// <param name="cardNumber">The card number to search for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The client if found; otherwise, null.</returns>
    Task<Client?> GetByNumberAsync(string cardNumber, CancellationToken ct);

    /// <summary>
    /// Gets a client by ID.
    /// </summary>
    /// <param name="id">The client ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The client if found; otherwise, null.</returns>
    Task<Client?> GetByIdAsync(Guid id, CancellationToken ct);

    /// <summary>
    /// Adds a new client.
    /// </summary>
    /// <param name="client">The client to add.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddAsync(Client client, CancellationToken ct);

    /// <summary>
    /// Updates an existing client.
    /// </summary>
    /// <param name="client">The client to update.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task UpdateAsync(Client client, CancellationToken ct);
}
