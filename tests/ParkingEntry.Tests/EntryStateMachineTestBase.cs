// <copyright file="EntryStateMachineTestBase.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using ParkingEntry.Core.Application.Handlers;
using ParkingEntry.Core.Application.Services;
using ParkingEntry.Core.Application.StateMachine;
using ParkingEntry.Core.Domain;
using ParkingEntry.Core.Infrastructure;

namespace ParkingEntry.Tests;

/// <summary>
/// Базовый класс для тестов конечного автомата.
/// Позволяет настраивать начальное состояние и моки сервисов.
/// </summary>
public abstract class EntryStateMachineTestBase
{
    protected IMediator Mediator { get; private set; } = null!;

    protected IStateContext StateContext { get; private set; } = null!;

    protected Mock<IBarrierService> BarrierServiceMock { get; private set; } = null!;

    protected Mock<ILightService> LightServiceMock { get; private set; } = null!;

    protected Mock<IMifareService> MifareServiceMock { get; private set; } = null!;

    protected Mock<IAccessCheckService> AccessCheckServiceMock { get; private set; } = null!;

    protected Mock<IDebtCheckService> DebtCheckServiceMock { get; private set; } = null!;

    protected Mock<ITimeoutManager> TimeoutManagerMock { get; private set; } = null!;

    /// <summary>
    /// Настроить тестовое окружение.
    /// </summary>
    /// <param name="initialState">Начальное состояние (по умолчанию Idle).</param>
    /// <param name="initialCardNumber">Начальный номер карты.</param>
    protected void Setup(EntryState initialState = EntryState.Idle, string? initialCardNumber = null)
    {
        // Создаем моки сервисов
        BarrierServiceMock = new Mock<IBarrierService>();
        LightServiceMock = new Mock<ILightService>();
        MifareServiceMock = new Mock<IMifareService>();
        AccessCheckServiceMock = new Mock<IAccessCheckService>();
        DebtCheckServiceMock = new Mock<IDebtCheckService>();
        TimeoutManagerMock = new Mock<ITimeoutManager>();

        // Настройка дефолтных значений для моков
        BarrierServiceMock.Setup(x => x.CurrentStatus).Returns(BarrierStatus.Closed);
        BarrierServiceMock.Setup(x => x.OpenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(BarrierStatus.Opened);
        BarrierServiceMock.Setup(x => x.CloseAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(BarrierStatus.Closed);

        LightServiceMock.Setup(x => x.CurrentStatus).Returns(new LightStatus(LightColor.Green, true));
        LightServiceMock.Setup(x => x.SetColorAsync(It.IsAny<LightColor>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LightColor color, CancellationToken _) => new LightStatus(color, true));

        MifareServiceMock.Setup(x => x.CurrentStatus).Returns(MifareStatus.Idle);
        MifareServiceMock.Setup(x => x.StartSearchAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(MifareStatus.Searching);
        MifareServiceMock.Setup(x => x.StopSearchAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(MifareStatus.Idle);

        AccessCheckServiceMock.Setup(x => x.CheckAccessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, null as string));

        DebtCheckServiceMock.Setup(x => x.CalculateDebtAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0m);

        // Создаем ServiceCollection и регистрируем все сервисы
        var services = new ServiceCollection();

        // Регистрируем MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(VehicleApproachedHandler).Assembly);
        });

        // Регистрируем моки как сервисы
        services.AddSingleton(BarrierServiceMock.Object);
        services.AddSingleton(LightServiceMock.Object);
        services.AddSingleton(MifareServiceMock.Object);
        services.AddSingleton(AccessCheckServiceMock.Object);
        services.AddSingleton(DebtCheckServiceMock.Object);
        services.AddSingleton(TimeoutManagerMock.Object);

        // Создаем контекст состояния
        StateContext = new StateContext();
        services.AddSingleton(StateContext);

        // Настраиваем начальное состояние если нужно
        if (initialState != EntryState.Idle)
        {
            StateContext.SetState(initialState);
        }

        if (initialCardNumber != null)
        {
            StateContext.SetCardNumber(initialCardNumber);
        }

        // Добавляем логирование для отладки
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Создаем ServiceProvider
        var serviceProvider = services.BuildServiceProvider();

        // Получаем Mediator
        Mediator = serviceProvider.GetRequiredService<IMediator>();
    }

    /// <summary>
    /// Проверить что состояние изменилось на ожидаемое.
    /// </summary>
    /// <param name="expectedState">Ожидаемое состояние.</param>
    protected void AssertState(EntryState expectedState)
    {
        FluentAssertions.AssertionExtensions.Should(StateContext.CurrentState).Be(expectedState);
    }

    /// <summary>
    /// Проверить что номер карты установлен.
    /// </summary>
    /// <param name="expectedCardNumber">Ожидаемый номер карты.</param>
    protected void AssertCardNumber(string? expectedCardNumber)
    {
        FluentAssertions.AssertionExtensions.Should(StateContext.CurrentCardNumber).Be(expectedCardNumber);
    }
}
