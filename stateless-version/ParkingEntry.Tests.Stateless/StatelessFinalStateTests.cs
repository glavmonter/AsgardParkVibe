// <copyright file="StatelessFinalStateTests.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ParkingEntry.Core.Stateless.Application.Commands;
using ParkingEntry.Core.Stateless.Application.Services;
using ParkingEntry.Core.Stateless.Domain;
using ParkingEntry.Core.Stateless.Infrastructure;

namespace ParkingEntry.Tests.Stateless;

/// <summary>
/// Тесты Stateless версии с возможностью начинать с любого состояния.
/// </summary>
[TestFixture]
public class StatelessFinalStateTests
{
    private Mock<IBarrierService> _barrierServiceMock = null!;
    private Mock<ILightService> _lightServiceMock = null!;
    private Mock<IMifareService> _mifareServiceMock = null!;
    private Mock<IAccessCheckService> _accessCheckServiceMock = null!;

    [SetUp]
    public void SetUp()
    {
        _barrierServiceMock = new Mock<IBarrierService>();
        _lightServiceMock = new Mock<ILightService>();
        _mifareServiceMock = new Mock<IMifareService>();
        _accessCheckServiceMock = new Mock<IAccessCheckService>();

        // Настройка дефолтных значений
        _barrierServiceMock.Setup(x => x.OpenAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(BarrierStatus.Opened);
        _barrierServiceMock.Setup(x => x.CloseAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(BarrierStatus.Closed);

        _lightServiceMock.Setup(x => x.SetColorAsync(It.IsAny<LightColor>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LightColor color, CancellationToken _) => new LightStatus(color, true));

        _mifareServiceMock.Setup(x => x.StartSearchAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(MifareStatus.Searching);
        _mifareServiceMock.Setup(x => x.StopSearchAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(MifareStatus.Idle);

        _accessCheckServiceMock.Setup(x => x.CheckAccessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((true, null as string));
    }

    /// <summary>
    /// Создать координатор с автоматическим переводом в нужное состояние.
    /// </summary>
    private async Task<EntryCoordinator> CreateCoordinatorAtStateAsync(
        EntryState targetState,
        string? cardNumber = null)
    {
        var services = new ServiceCollection();

        // Регистрируем MediatR и handlers
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(EntryStateMachine).Assembly);
        });

        // Регистрируем моки
        services.AddSingleton(_barrierServiceMock.Object);
        services.AddSingleton(_lightServiceMock.Object);
        services.AddSingleton(_mifareServiceMock.Object);
        services.AddSingleton(_accessCheckServiceMock.Object);
        services.AddSingleton(Mock.Of<IDebtCheckService>());

        // Логирование
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Регистрируем StateMachine и Coordinator
        services.AddSingleton<EntryStateMachine>();
        services.AddSingleton<EntryCoordinator>();

        var provider = services.BuildServiceProvider();
        var coordinator = provider.GetRequiredService<EntryCoordinator>();

        // Переводим в нужное состояние
        switch (targetState)
        {
            case EntryState.Idle:
                // Уже в Idle
                break;

            case EntryState.ReadingCard:
                await coordinator.HandleVehicleApproachedAsync();
                break;

            case EntryState.WaitingPassage:
                // Idle → ReadingCard → CheckingAccess → OpeningBarrier → WaitingPassage
                await coordinator.HandleVehicleApproachedAsync();
                await coordinator.HandleCardReadAsync(cardNumber ?? "TEST_CARD");
                // CheckingAccess автоматически переходит в OpeningBarrier
                // OpeningBarrier автоматически переходит в WaitingPassage
                await Task.Delay(100); // Даем время на async переходы
                break;

            case EntryState.AccessDenied:
                // Настраиваем отказ в доступе
                _accessCheckServiceMock
                    .Setup(x => x.CheckAccessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((false, "Test denial"));

                await coordinator.HandleVehicleApproachedAsync();
                await coordinator.HandleCardReadAsync("DENIED_CARD");
                await Task.Delay(100);
                break;

            default:
                throw new NotSupportedException($"Состояние {targetState} не поддерживается в тестах");
        }

        return coordinator;
    }

    [Test]
    public async Task VehiclePassed_FromWaitingPassage_ShouldCompleteSuccessfully()
    {
        // Arrange - НАЧИНАЕМ С WaitingPassage
        var coordinator = await CreateCoordinatorAtStateAsync(EntryState.WaitingPassage, "CARD123");

        coordinator.CurrentState.Should().Be(EntryState.WaitingPassage, "должны быть в WaitingPassage");

        // Act - автомобиль проезжает
        await coordinator.HandleVehiclePassedAsync();

        // Даем время на автоматический переход через ClosingBarrier → Idle
        await Task.Delay(100);

        // Assert
        coordinator.CurrentState.Should().Be(EntryState.Idle, "должны вернуться в Idle");

        _barrierServiceMock.Verify(
            x => x.CloseAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "шлагбаум должен быть закрыт");

        _lightServiceMock.Verify(
            x => x.SetColorAsync(LightColor.Green, It.IsAny<CancellationToken>()),
            Times.Once,
            "светофор должен стать зеленым");
    }

    [Test]
    public async Task VehicleReversed_FromWaitingPassage_ShouldCompleteSuccessfully()
    {
        // Arrange - НАЧИНАЕМ С WaitingPassage
        var coordinator = await CreateCoordinatorAtStateAsync(EntryState.WaitingPassage, "CARD456");

        // Act
        await coordinator.HandleVehicleReversedAsync();
        await Task.Delay(100);

        // Assert
        coordinator.CurrentState.Should().Be(EntryState.Idle);

        _barrierServiceMock.Verify(
            x => x.CloseAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _lightServiceMock.Verify(
            x => x.SetColorAsync(LightColor.Green, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task VehicleLeft_FromAccessDenied_ShouldReturnToIdle()
    {
        // Arrange - НАЧИНАЕМ С AccessDenied
        var coordinator = await CreateCoordinatorAtStateAsync(EntryState.AccessDenied);

        coordinator.CurrentState.Should().Be(EntryState.AccessDenied);

        // Act
        await coordinator.HandleVehicleLeftAsync();

        // Assert
        coordinator.CurrentState.Should().Be(EntryState.Idle);
    }

    [Test]
    public async Task VehicleLeft_FromReadingCard_ShouldReturnToIdle()
    {
        // Arrange - НАЧИНАЕМ С ReadingCard
        var coordinator = await CreateCoordinatorAtStateAsync(EntryState.ReadingCard);

        coordinator.CurrentState.Should().Be(EntryState.ReadingCard);

        // Act
        await coordinator.HandleVehicleLeftAsync();

        // Assert
        coordinator.CurrentState.Should().Be(EntryState.Idle);

        _mifareServiceMock.Verify(
            x => x.StopSearchAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "поиск карты должен быть остановлен");
    }

    [Test]
    public async Task FullScenario_WithStateless_ShouldWorkCorrectly()
    {
        // Arrange - начинаем с Idle
        var coordinator = await CreateCoordinatorAtStateAsync(EntryState.Idle);

        // Act & Assert - полный сценарий

        // 1. Автомобиль подъезжает
        await coordinator.HandleVehicleApproachedAsync();
        coordinator.CurrentState.Should().Be(EntryState.ReadingCard);

        _lightServiceMock.Verify(x => x.SetColorAsync(LightColor.Red, It.IsAny<CancellationToken>()), Times.Once);
        _mifareServiceMock.Verify(x => x.StartSearchAsync(It.IsAny<CancellationToken>()), Times.Once);

        // 2. Карта прочитана
        await coordinator.HandleCardReadAsync("VALID_CARD");
        await Task.Delay(100); // Ждем автоматические переходы

        coordinator.CurrentState.Should().Be(EntryState.WaitingPassage);
        _barrierServiceMock.Verify(x => x.OpenAsync(It.IsAny<CancellationToken>()), Times.Once);

        // 3. Автомобиль проезжает
        await coordinator.HandleVehiclePassedAsync();
        await Task.Delay(100);

        coordinator.CurrentState.Should().Be(EntryState.Idle);
        _barrierServiceMock.Verify(x => x.CloseAsync(It.IsAny<CancellationToken>()), Times.Once);
        _lightServiceMock.Verify(x => x.SetColorAsync(LightColor.Green, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public void StateMachine_ShouldGenerateDotGraph()
    {
        // Arrange
        var coordinator = CreateCoordinatorAtStateAsync(EntryState.Idle).GetAwaiter().GetResult();

        // Act
        var dotGraph = coordinator.GetStateDiagram();

        // Assert
        dotGraph.Should().NotBeNullOrEmpty();
        dotGraph.Should().Contain("digraph");
        dotGraph.Should().Contain("Idle");
        dotGraph.Should().Contain("ReadingCard");
        dotGraph.Should().Contain("WaitingPassage");
    }
}
