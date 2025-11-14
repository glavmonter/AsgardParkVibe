// <copyright file="LightBehaviorTests.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using FluentAssertions;
using Moq;
using NUnit.Framework;
using ParkingEntry.Core.Domain;
using ParkingEntry.Core.Domain.Events;

namespace ParkingEntry.Tests;

/// <summary>
/// Тесты поведения светофора.
/// </summary>
[TestFixture]
public class LightBehaviorTests : EntryStateMachineTestBase
{
    [SetUp]
    public void SetUp()
    {
        Setup();
    }

    [Test]
    public async Task VehicleApproached_ShouldSwitchLightToRed()
    {
        // Arrange - начинаем в состоянии Idle

        // Act
        await Mediator.Publish(new VehicleApproached());

        // Assert
        LightServiceMock.Verify(
            x => x.SetColorAsync(LightColor.Red, It.IsAny<CancellationToken>()),
            Times.Once,
            "Светофор должен переключиться на красный при подъезде автомобиля");
    }

    [Test]
    public async Task VehicleLeft_FromReadingCard_ShouldSwitchLightToGreen()
    {
        // Arrange - начинаем в состоянии ReadingCard
        Setup(EntryState.ReadingCard);

        // Act
        await Mediator.Publish(new VehicleLeft());

        // Assert
        LightServiceMock.Verify(
            x => x.SetColorAsync(LightColor.Green, It.IsAny<CancellationToken>()),
            Times.Once,
            "Светофор должен переключиться на зеленый когда автомобиль уехал");
    }

    [Test]
    public async Task VehiclePassed_ShouldSwitchLightToGreen()
    {
        // Arrange - начинаем в состоянии WaitingPassage
        Setup(EntryState.WaitingPassage, "TEST123");

        // Act
        await Mediator.Publish(new VehiclePassed());

        // Assert
        LightServiceMock.Verify(
            x => x.SetColorAsync(LightColor.Green, It.IsAny<CancellationToken>()),
            Times.Once,
            "Светофор должен переключиться на зеленый после проезда");
    }

    [Test]
    public async Task VehicleReversed_ShouldSwitchLightToGreen()
    {
        // Arrange - начинаем в состоянии WaitingPassage
        Setup(EntryState.WaitingPassage, "TEST123");

        // Act
        await Mediator.Publish(new VehicleReversed());

        // Assert
        LightServiceMock.Verify(
            x => x.SetColorAsync(LightColor.Green, It.IsAny<CancellationToken>()),
            Times.Once,
            "Светофор должен переключиться на зеленый когда автомобиль уехал назад");
    }

    [Test]
    public async Task FullScenario_SuccessfulEntry_LightShouldChangeCorrectly()
    {
        // Arrange
        var cardNumber = "CARD12345";

        // Act & Assert

        // 1. Автомобиль подъезжает - красный свет
        await Mediator.Publish(new VehicleApproached());
        LightServiceMock.Verify(x => x.SetColorAsync(LightColor.Red, It.IsAny<CancellationToken>()), Times.Once);

        // 2. Карта прочитана
        await Mediator.Publish(new CardRead(cardNumber));

        // 3. Автомобиль проезжает - зеленый свет
        await Mediator.Publish(new VehiclePassed());
        LightServiceMock.Verify(x => x.SetColorAsync(LightColor.Green, It.IsAny<CancellationToken>()), Times.Once);
    }
}
