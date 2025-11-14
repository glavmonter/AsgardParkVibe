// <copyright file="EquipmentStatus.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

namespace ParkingEntry.Core.Domain;

/// <summary>
/// Статус шлагбаума.
/// </summary>
public enum BarrierStatus
{
    /// <summary>
    /// Закрыт.
    /// </summary>
    Closed,

    /// <summary>
    /// Открывается.
    /// </summary>
    Opening,

    /// <summary>
    /// Открыт.
    /// </summary>
    Opened,

    /// <summary>
    /// Закрывается.
    /// </summary>
    Closing,

    /// <summary>
    /// Ошибка.
    /// </summary>
    Error,
}

/// <summary>
/// Цвет светофора.
/// </summary>
public enum LightColor
{
    /// <summary>
    /// Все выключены.
    /// </summary>
    Off,

    /// <summary>
    /// Красный.
    /// </summary>
    Red,

    /// <summary>
    /// Зеленый.
    /// </summary>
    Green,

    /// <summary>
    /// Оба горят.
    /// </summary>
    Both,
}

/// <summary>
/// Статус светофора.
/// </summary>
public record LightStatus(LightColor Color, bool IsHealthy);

/// <summary>
/// Статус считывателя карт.
/// </summary>
public enum MifareStatus
{
    /// <summary>
    /// Ожидание.
    /// </summary>
    Idle,

    /// <summary>
    /// Поиск карты.
    /// </summary>
    Searching,

    /// <summary>
    /// Запись карты.
    /// </summary>
    Writing,

    /// <summary>
    /// Ошибка.
    /// </summary>
    Error,
}

/// <summary>
/// Уровень отказа оборудования.
/// </summary>
public enum HealthLevel
{
    /// <summary>
    /// Работает нормально.
    /// </summary>
    Healthy,

    /// <summary>
    /// Мягкий отказ (можно работать с ограничениями).
    /// </summary>
    Degraded,

    /// <summary>
    /// Аварийный отказ (нельзя работать).
    /// </summary>
    Critical,
}

/// <summary>
/// Состояние здоровья оборудования.
/// </summary>
public record HealthStatus(
    string EquipmentName,
    HealthLevel Level,
    string? ErrorMessage = null,
    DateTime Timestamp = default);
