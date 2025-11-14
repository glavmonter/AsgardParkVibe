// <copyright file="DomainEvents.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using MediatR;

namespace ParkingEntry.Core.Domain.Events;

/// <summary>
/// Базовое доменное событие.
/// </summary>
public abstract record DomainEvent : INotification
{
    /// <summary>
    /// Уникальный идентификатор события.
    /// </summary>
    public Guid EventId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Время возникновения события.
    /// </summary>
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Автомобиль подъехал к шлагбауму.
/// </summary>
public sealed record VehicleApproached : DomainEvent;

/// <summary>
/// Автомобиль уехал от шлагбаума.
/// </summary>
public sealed record VehicleLeft : DomainEvent;

/// <summary>
/// Автомобиль проехал через шлагбаум полностью.
/// </summary>
public sealed record VehiclePassed : DomainEvent;

/// <summary>
/// Автомобиль заехал за шлагбаум, но уехал назад.
/// </summary>
public sealed record VehicleReversed : DomainEvent;

/// <summary>
/// Карта прочитана.
/// </summary>
/// <param name="CardNumber">Номер карты.</param>
public sealed record CardRead(string CardNumber) : DomainEvent;

/// <summary>
/// Карта записана.
/// </summary>
/// <param name="CardNumber">Номер карты.</param>
public sealed record CardWritten(string CardNumber) : DomainEvent;

/// <summary>
/// Таймаут операции.
/// </summary>
/// <param name="OperationName">Название операции.</param>
public sealed record OperationTimeout(string OperationName) : DomainEvent;

/// <summary>
/// Изменилось состояние здоровья оборудования.
/// </summary>
/// <param name="Status">Новый статус.</param>
public sealed record EquipmentHealthChanged(HealthStatus Status) : DomainEvent;
