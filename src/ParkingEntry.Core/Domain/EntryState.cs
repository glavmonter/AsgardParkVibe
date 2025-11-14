// <copyright file="EntryState.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

namespace ParkingEntry.Core.Domain;

/// <summary>
/// Состояния конечного автомата въездной стойки.
/// </summary>
public enum EntryState
{
    /// <summary>
    /// Ожидание автомобиля.
    /// </summary>
    Idle,

    /// <summary>
    /// Чтение карты.
    /// </summary>
    ReadingCard,

    /// <summary>
    /// Проверка прав доступа.
    /// </summary>
    CheckingAccess,

    /// <summary>
    /// Проверка задолженности.
    /// </summary>
    CheckingDebt,

    /// <summary>
    /// Открытие шлагбаума.
    /// </summary>
    OpeningBarrier,

    /// <summary>
    /// Ожидание проезда через шлагбаум.
    /// </summary>
    WaitingPassage,

    /// <summary>
    /// Закрытие шлагбаума после проезда.
    /// </summary>
    ClosingBarrier,

    /// <summary>
    /// Доступ запрещен.
    /// </summary>
    AccessDenied,

    /// <summary>
    /// Ошибка оборудования.
    /// </summary>
    EquipmentError,
}
