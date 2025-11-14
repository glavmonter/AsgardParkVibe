// <copyright file="StateContext.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using ParkingEntry.Core.Application.StateMachine;
using ParkingEntry.Core.Domain;

namespace ParkingEntry.Core.Infrastructure;

/// <summary>
/// Реализация контекста состояния (Singleton).
/// </summary>
public sealed class StateContext : IStateContext
{
    private readonly object _lock = new();
    private EntryState _currentState = EntryState.Idle;
    private string? _currentCardNumber;

    /// <inheritdoc/>
    public EntryState CurrentState
    {
        get
        {
            lock (_lock)
            {
                return _currentState;
            }
        }
    }

    /// <inheritdoc/>
    public string? CurrentCardNumber
    {
        get
        {
            lock (_lock)
            {
                return _currentCardNumber;
            }
        }
    }

    /// <inheritdoc/>
    public void SetState(EntryState state)
    {
        lock (_lock)
        {
            _currentState = state;
        }
    }

    /// <inheritdoc/>
    public void SetCardNumber(string? cardNumber)
    {
        lock (_lock)
        {
            _currentCardNumber = cardNumber;
        }
    }

    /// <inheritdoc/>
    public void Reset()
    {
        lock (_lock)
        {
            _currentState = EntryState.Idle;
            _currentCardNumber = null;
        }
    }
}
