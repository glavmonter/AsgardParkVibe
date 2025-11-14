// <copyright file="EntryTrigger.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

namespace ParkingEntry.Core.Stateless.Domain;

/// <summary>
/// Триггеры для переходов между состояниями въездной стойки.
/// </summary>
public enum EntryTrigger
{
    /// <summary>
    /// Автомобиль подъехал к шлагбауму.
    /// </summary>
    VehicleApproached,

    /// <summary>
    /// Карта прочитана успешно.
    /// </summary>
    CardRead,

    /// <summary>
    /// Карта не прочитана (таймаут).
    /// </summary>
    CardReadTimeout,

    /// <summary>
    /// Доступ разрешен.
    /// </summary>
    AccessGranted,

    /// <summary>
    /// Доступ запрещен.
    /// </summary>
    AccessDenied,

    /// <summary>
    /// Шлагбаум открыт.
    /// </summary>
    BarrierOpened,

    /// <summary>
    /// Автомобиль проехал через шлагбаум.
    /// </summary>
    VehiclePassed,

    /// <summary>
    /// Автомобиль уехал назад.
    /// </summary>
    VehicleReversed,

    /// <summary>
    /// Автомобиль уехал от шлагбаума (до проезда).
    /// </summary>
    VehicleLeft,

    /// <summary>
    /// Шлагбаум закрыт.
    /// </summary>
    BarrierClosed,

    /// <summary>
    /// Ошибка оборудования.
    /// </summary>
    EquipmentError,

    /// <summary>
    /// Восстановление после ошибки.
    /// </summary>
    Reset,
}
