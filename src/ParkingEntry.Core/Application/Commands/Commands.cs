// <copyright file="Commands.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using MediatR;
using ParkingEntry.Core.Domain;

namespace ParkingEntry.Core.Application.Commands;

/// <summary>
/// Базовая команда для работы с оборудованием.
/// </summary>
/// <typeparam name="TResult">Тип результата.</typeparam>
public abstract record EquipmentCommand<TResult> : IRequest<TResult>;

// ==================== Шлагбаум ====================

/// <summary>
/// Открыть шлагбаум.
/// </summary>
public sealed record OpenBarrierCommand : EquipmentCommand<BarrierStatus>;

/// <summary>
/// Закрыть шлагбаум.
/// </summary>
public sealed record CloseBarrierCommand : EquipmentCommand<BarrierStatus>;

// ==================== Светофор ====================

/// <summary>
/// Переключить светофор.
/// </summary>
/// <param name="Color">Требуемый цвет.</param>
public sealed record SetLightCommand(LightColor Color) : EquipmentCommand<LightStatus>;

// ==================== Считыватель карт ====================

/// <summary>
/// Начать поиск карты.
/// </summary>
public sealed record StartCardSearchCommand : EquipmentCommand<MifareStatus>;

/// <summary>
/// Остановить поиск карты.
/// </summary>
public sealed record StopCardSearchCommand : EquipmentCommand<MifareStatus>;

/// <summary>
/// Записать карту.
/// </summary>
/// <param name="CardNumber">Номер карты для записи.</param>
public sealed record WriteCardCommand(string CardNumber) : EquipmentCommand<MifareStatus>;

/// <summary>
/// Остановить запись карты.
/// </summary>
public sealed record StopCardWriteCommand : EquipmentCommand<MifareStatus>;

// ==================== Бизнес-логика ====================

/// <summary>
/// Проверить права доступа клиента.
/// </summary>
/// <param name="ClientNumber">Номер клиента.</param>
public sealed record CheckAccessCommand(string ClientNumber) : IRequest<AccessCheckResult>;

/// <summary>
/// Результат проверки доступа.
/// </summary>
/// <param name="IsAllowed">Разрешен ли доступ.</param>
/// <param name="Reason">Причина отказа.</param>
public record AccessCheckResult(bool IsAllowed, string? Reason = null);

/// <summary>
/// Проверить задолженность клиента.
/// </summary>
/// <param name="ClientNumber">Номер клиента.</param>
public sealed record CheckDebtCommand(string ClientNumber) : IRequest<decimal>;
