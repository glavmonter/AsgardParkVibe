// <copyright file="FullScenarioTests.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using FluentAssertions;
using Moq;
using NUnit.Framework;
using ParkingEntry.Core.Domain;
using ParkingEntry.Core.Domain.Events;

namespace ParkingEntry.Tests;

/// <summary>
/// Тесты полного сценария работы системы.
/// </summary>
[TestFixture]
public class FullScenarioTests : EntryStateMachineTestBase
{
    [SetUp]
    public void SetUp()
    {
        Setup();
    }

    [Test]
    public async Task SuccessfulEntry_FullFlow_ShouldWorkCorrectly()
    {
        // Arrange
        var cardNumber = "VALID_CARD";

        // Act & Assert - проходим весь сценарий

        // 1. Автомобиль подъезжает
        await Mediator.Publish(new VehicleApproached());

        AssertState(EntryState.ReadingCard);
        MifareServiceMock.Verify(x => x.StartSearchAsync(It.IsAny<CancellationToken>()), Times.Once);
        LightServiceMock.Verify(x => x.SetColorAsync(LightColor.Red, It.IsAny<CancellationToken>()), Times.Once);

        // 2. Карта прочитана
        await Mediator.Publish(new CardRead(cardNumber));

        AssertCardNumber(cardNumber);
        MifareServiceMock.Verify(x => x.StopSearchAsync(It.IsAny<CancellationToken>()), Times.Once);

        // После проверки доступа должен открыться шлагбаум
        BarrierServiceMock.Verify(x => x.OpenAsync(It.IsAny<CancellationToken>()), Times.Once);
        AssertState(EntryState.WaitingPassage);

        // 3. Автомобиль проезжает
        await Mediator.Publish(new VehiclePassed());

        BarrierServiceMock.Verify(x => x.CloseAsync(It.IsAny<CancellationToken>()), Times.Once);
        LightServiceMock.Verify(x => x.SetColorAsync(LightColor.Green, It.IsAny<CancellationToken>()), Times.Once);
        AssertState(EntryState.Idle);
        AssertCardNumber(null);
    }

    [Test]
    public async Task DeniedEntry_FullFlow_ShouldHandleCorrectly()
    {
        // Arrange
        var cardNumber = "DENIED";

        // Настраиваем мок для отказа в доступе
        AccessCheckServiceMock
            .Setup(x => x.CheckAccessAsync(cardNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((false, "Клиент в черном списке"));

        // Act & Assert

        // 1. Автомобиль подъезжает
        await Mediator.Publish(new VehicleApproached());
        AssertState(EntryState.ReadingCard);

        // 2. Карта прочитана
        await Mediator.Publish(new CardRead(cardNumber));

        // Должно быть состояние AccessDenied, шлагбаум НЕ должен открыться
        AssertState(EntryState.AccessDenied);
        BarrierServiceMock.Verify(x => x.OpenAsync(It.IsAny<CancellationToken>()), Times.Never);

        // 3. Автомобиль уезжает
        await Mediator.Publish(new VehicleLeft());

        AssertState(EntryState.Idle);
        LightServiceMock.Verify(x => x.SetColorAsync(LightColor.Green, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task VehicleReversed_AfterBarrierOpen_ShouldCloseBarrier()
    {
        // Arrange
        var cardNumber = "TEST_CARD";

        // Проходим до состояния WaitingPassage
        await Mediator.Publish(new VehicleApproached());
        await Mediator.Publish(new CardRead(cardNumber));

        AssertState(EntryState.WaitingPassage);
        BarrierServiceMock.Verify(x => x.OpenAsync(It.IsAny<CancellationToken>()), Times.Once);

        // Act - автомобиль уехал назад
        await Mediator.Publish(new VehicleReversed());

        // Assert
        BarrierServiceMock.Verify(x => x.CloseAsync(It.IsAny<CancellationToken>()), Times.Once);
        AssertState(EntryState.Idle);
    }

    [Test]
    public async Task VehicleLeft_BeforeCardRead_ShouldResetToIdle()
    {
        // Arrange
        await Mediator.Publish(new VehicleApproached());
        AssertState(EntryState.ReadingCard);

        // Act - автомобиль уехал до того как карта была прочитана
        await Mediator.Publish(new VehicleLeft());

        // Assert
        AssertState(EntryState.Idle);
        MifareServiceMock.Verify(x => x.StopSearchAsync(It.IsAny<CancellationToken>()), Times.Once);
        LightServiceMock.Verify(x => x.SetColorAsync(LightColor.Green, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ConsecutiveVehicles_ShouldProcessIndependently()
    {
        // Arrange & Act - первый автомобиль
        await Mediator.Publish(new VehicleApproached());
        await Mediator.Publish(new CardRead("CARD_001"));
        await Mediator.Publish(new VehiclePassed());

        AssertState(EntryState.Idle);
        AssertCardNumber(null);

        // Второй автомобиль
        await Mediator.Publish(new VehicleApproached());
        await Mediator.Publish(new CardRead("CARD_002"));
        await Mediator.Publish(new VehiclePassed());

        // Assert
        AssertState(EntryState.Idle);
        AssertCardNumber(null);

        // Проверяем количество вызовов
        BarrierServiceMock.Verify(x => x.OpenAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        BarrierServiceMock.Verify(x => x.CloseAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
        MifareServiceMock.Verify(x => x.StartSearchAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
