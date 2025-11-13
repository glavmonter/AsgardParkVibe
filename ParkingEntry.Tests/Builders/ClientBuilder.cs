using ParkingEntry.Domain.Entities;

namespace ParkingEntry.Tests.Builders;

/// <summary>
/// Builder for creating test Client instances.
/// </summary>
public class ClientBuilder
{
    private string _cardNumber = "TEST_CARD";
    private bool _hasActiveContract = true;
    private bool _isBlocked = false;
    private decimal _debt = 0;
    private string _name = "Test Client";

    /// <summary>
    /// Creates a new instance of the builder.
    /// </summary>
    /// <returns>A new ClientBuilder instance.</returns>
    public static ClientBuilder Create() => new();

    /// <summary>
    /// Sets the card number.
    /// </summary>
    /// <param name="cardNumber">The card number.</param>
    /// <returns>The builder instance.</returns>
    public ClientBuilder WithNumber(string cardNumber)
    {
        _cardNumber = cardNumber;
        return this;
    }

    /// <summary>
    /// Sets the client name.
    /// </summary>
    /// <param name="name">The client name.</param>
    /// <returns>The builder instance.</returns>
    public ClientBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    /// Sets the client to have an active contract.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public ClientBuilder WithActiveContract()
    {
        _hasActiveContract = true;
        return this;
    }

    /// <summary>
    /// Sets the client to have no active contract.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public ClientBuilder WithoutActiveContract()
    {
        _hasActiveContract = false;
        return this;
    }

    /// <summary>
    /// Sets the client as blocked.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public ClientBuilder Blocked()
    {
        _isBlocked = true;
        return this;
    }

    /// <summary>
    /// Sets the client as not blocked.
    /// </summary>
    /// <returns>The builder instance.</returns>
    public ClientBuilder NotBlocked()
    {
        _isBlocked = false;
        return this;
    }

    /// <summary>
    /// Sets the client debt amount.
    /// </summary>
    /// <param name="debt">The debt amount.</param>
    /// <returns>The builder instance.</returns>
    public ClientBuilder WithDebt(decimal debt)
    {
        _debt = debt;
        return this;
    }

    /// <summary>
    /// Builds the Client instance.
    /// </summary>
    /// <returns>A configured Client instance.</returns>
    public Client Build()
    {
        return new Client
        {
            Id = Guid.NewGuid(),
            CardNumber = _cardNumber,
            Name = _name,
            HasActiveContract = _hasActiveContract,
            IsBlocked = _isBlocked,
            Debt = _debt,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
