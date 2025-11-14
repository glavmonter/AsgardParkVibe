// <copyright file="MockServices.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using Microsoft.Extensions.Logging;
using ParkingEntry.Core.Application.Services;
using ParkingEntry.Core.Domain;

namespace ParkingEntry.Api.Services;

/// <summary>
/// Mock-реализация сервиса шлагбаума.
/// </summary>
public sealed class MockBarrierService : IBarrierService
{
    private readonly ILogger<MockBarrierService> _logger;
    private BarrierStatus _status = BarrierStatus.Closed;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MockBarrierService"/>.
    /// </summary>
    public MockBarrierService(ILogger<MockBarrierService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public BarrierStatus CurrentStatus => _status;

    /// <inheritdoc/>
    public async Task<BarrierStatus> OpenAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Открываем шлагбаум...");
        _status = BarrierStatus.Opening;
        await Task.Delay(1000, cancellationToken); // Имитация работы
        _status = BarrierStatus.Opened;
        _logger.LogInformation("Шлагбаум открыт");
        return _status;
    }

    /// <inheritdoc/>
    public async Task<BarrierStatus> CloseAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Закрываем шлагбаум...");
        _status = BarrierStatus.Closing;
        await Task.Delay(1000, cancellationToken); // Имитация работы
        _status = BarrierStatus.Closed;
        _logger.LogInformation("Шлагбаум закрыт");
        return _status;
    }
}

/// <summary>
/// Mock-реализация сервиса светофора.
/// </summary>
public sealed class MockLightService : ILightService
{
    private readonly ILogger<MockLightService> _logger;
    private LightStatus _status = new(LightColor.Green, true);

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MockLightService"/>.
    /// </summary>
    public MockLightService(ILogger<MockLightService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public LightStatus CurrentStatus => _status;

    /// <inheritdoc/>
    public Task<LightStatus> SetColorAsync(LightColor color, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Переключаем светофор на {Color}", color);
        _status = new LightStatus(color, true);
        return Task.FromResult(_status);
    }
}

/// <summary>
/// Mock-реализация сервиса считывателя карт.
/// </summary>
public sealed class MockMifareService : IMifareService
{
    private readonly ILogger<MockMifareService> _logger;
    private MifareStatus _status = MifareStatus.Idle;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MockMifareService"/>.
    /// </summary>
    public MockMifareService(ILogger<MockMifareService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public MifareStatus CurrentStatus => _status;

    /// <inheritdoc/>
    public Task<MifareStatus> StartSearchAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Начинаем поиск карты");
        _status = MifareStatus.Searching;
        return Task.FromResult(_status);
    }

    /// <inheritdoc/>
    public Task<MifareStatus> StopSearchAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Останавливаем поиск карты");
        _status = MifareStatus.Idle;
        return Task.FromResult(_status);
    }

    /// <inheritdoc/>
    public Task<MifareStatus> WriteCardAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Начинаем запись карты {CardNumber}", cardNumber);
        _status = MifareStatus.Writing;
        return Task.FromResult(_status);
    }

    /// <inheritdoc/>
    public Task<MifareStatus> StopWriteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Останавливаем запись карты");
        _status = MifareStatus.Idle;
        return Task.FromResult(_status);
    }
}

/// <summary>
/// Mock-реализация сервиса проверки доступа.
/// </summary>
public sealed class MockAccessCheckService : IAccessCheckService
{
    private readonly ILogger<MockAccessCheckService> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MockAccessCheckService"/>.
    /// </summary>
    public MockAccessCheckService(ILogger<MockAccessCheckService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<(bool IsAllowed, string? Reason)> CheckAccessAsync(
        string clientNumber,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Проверяем доступ для клиента {ClientNumber}", clientNumber);

        // Простая логика: разрешаем доступ если номер карты не "DENIED"
        var isAllowed = clientNumber != "DENIED";
        var reason = isAllowed ? null : "Клиент в черном списке";

        return Task.FromResult((isAllowed, reason));
    }
}

/// <summary>
/// Mock-реализация сервиса проверки задолженности.
/// </summary>
public sealed class MockDebtCheckService : IDebtCheckService
{
    private readonly ILogger<MockDebtCheckService> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="MockDebtCheckService"/>.
    /// </summary>
    public MockDebtCheckService(ILogger<MockDebtCheckService> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc/>
    public Task<decimal> CalculateDebtAsync(string clientNumber, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Рассчитываем задолженность для клиента {ClientNumber}", clientNumber);

        // Возвращаем случайную задолженность для демонстрации
        return Task.FromResult(Random.Shared.Next(0, 1000) / 100m);
    }
}
