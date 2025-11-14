// <copyright file="IEquipmentServices.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using ParkingEntry.Core.Domain;

namespace ParkingEntry.Core.Application.Services;

/// <summary>
/// Сервис управления шлагбаумом.
/// </summary>
public interface IBarrierService
{
    /// <summary>
    /// Открыть шлагбаум.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Статус шлагбаума.</returns>
    Task<BarrierStatus> OpenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Закрыть шлагбаум.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Статус шлагбаума.</returns>
    Task<BarrierStatus> CloseAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить текущий статус.
    /// </summary>
    BarrierStatus CurrentStatus { get; }
}

/// <summary>
/// Сервис управления светофором.
/// </summary>
public interface ILightService
{
    /// <summary>
    /// Установить цвет светофора.
    /// </summary>
    /// <param name="color">Требуемый цвет.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Статус светофора.</returns>
    Task<LightStatus> SetColorAsync(LightColor color, CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить текущий статус.
    /// </summary>
    LightStatus CurrentStatus { get; }
}

/// <summary>
/// Сервис чтения карт Mifare.
/// </summary>
public interface IMifareService
{
    /// <summary>
    /// Начать поиск карты.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Статус считывателя.</returns>
    Task<MifareStatus> StartSearchAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Остановить поиск карты.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Статус считывателя.</returns>
    Task<MifareStatus> StopSearchAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Записать карту.
    /// </summary>
    /// <param name="cardNumber">Номер карты.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Статус считывателя.</returns>
    Task<MifareStatus> WriteCardAsync(string cardNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Остановить запись карты.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Статус считывателя.</returns>
    Task<MifareStatus> StopWriteAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Получить текущий статус.
    /// </summary>
    MifareStatus CurrentStatus { get; }
}

/// <summary>
/// Сервис проверки прав доступа.
/// </summary>
public interface IAccessCheckService
{
    /// <summary>
    /// Проверить права доступа клиента.
    /// </summary>
    /// <param name="clientNumber">Номер клиента.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат проверки.</returns>
    Task<(bool IsAllowed, string? Reason)> CheckAccessAsync(string clientNumber, CancellationToken cancellationToken = default);
}

/// <summary>
/// Сервис проверки задолженности.
/// </summary>
public interface IDebtCheckService
{
    /// <summary>
    /// Рассчитать задолженность клиента.
    /// </summary>
    /// <param name="clientNumber">Номер клиента.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Сумма задолженности.</returns>
    Task<decimal> CalculateDebtAsync(string clientNumber, CancellationToken cancellationToken = default);
}
