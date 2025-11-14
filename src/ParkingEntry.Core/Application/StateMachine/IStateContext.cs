// <copyright file="IStateContext.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using ParkingEntry.Core.Domain;

namespace ParkingEntry.Core.Application.StateMachine;

/// <summary>
/// Контекст текущего состояния конечного автомата.
/// </summary>
public interface IStateContext
{
    /// <summary>
    /// Текущее состояние.
    /// </summary>
    EntryState CurrentState { get; }

    /// <summary>
    /// Номер карты текущего клиента.
    /// </summary>
    string? CurrentCardNumber { get; }

    /// <summary>
    /// Установить новое состояние.
    /// </summary>
    /// <param name="state">Новое состояние.</param>
    void SetState(EntryState state);

    /// <summary>
    /// Установить номер карты.
    /// </summary>
    /// <param name="cardNumber">Номер карты.</param>
    void SetCardNumber(string? cardNumber);

    /// <summary>
    /// Сбросить контекст в начальное состояние.
    /// </summary>
    void Reset();
}
