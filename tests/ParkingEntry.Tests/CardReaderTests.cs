// <copyright file="CardReaderTests.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using FluentAssertions;
using Moq;
using NUnit.Framework;
using ParkingEntry.Core.Domain;
using ParkingEntry.Core.Domain.Events;

namespace ParkingEntry.Tests;

/// <summary>
/// Тесты работы считывателя карт.
/// </summary>
[TestFixture]
public class CardReaderTests : EntryStateMachineTestBase
{
    [SetUp]
    public void SetUp()
    {
        Setup();
    }

    [Test]
    public async Task VehicleApproached_ShouldStartCardSearch()
    {
        // Arrange - начинаем в состоянии Idle

        // Act
        await Mediator.Publish(new VehicleApproached());

        // Assert
        MifareServiceMock.Verify(
            x => x.StartSearchAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "Должен начаться поиск карты при подъезде автомобиля");

        AssertState(EntryState.ReadingCard);
    }

    [Test]
    public async Task VehicleApproached_InWrongState_ShouldNotStartCardSearch()
    {
        // Arrange - начинаем в состоянии WaitingPassage
        Setup(EntryState.WaitingPassage);

        // Act
        await Mediator.Publish(new VehicleApproached());

        // Assert
        MifareServiceMock.Verify(
            x => x.StartSearchAsync(It.IsAny<CancellationToken>()),
            Times.Never,
            "Не должен начаться поиск карты если мы не в состоянии Idle");

        AssertState(EntryState.WaitingPassage);
    }

    [Test]
    public async Task CardRead_ShouldStopCardSearch()
    {
        // Arrange - начинаем в состоянии ReadingCard
        Setup(EntryState.ReadingCard);

        var cardNumber = "CARD123";

        // Act
        await Mediator.Publish(new CardRead(cardNumber));

        // Assert
        MifareServiceMock.Verify(
            x => x.StopSearchAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "Должен остановиться поиск карты после чтения");

        AssertCardNumber(cardNumber);
    }

    [Test]
    public async Task CardRead_InWrongState_ShouldBeIgnored()
    {
        // Arrange - начинаем в состоянии Idle
        Setup(EntryState.Idle);

        var cardNumber = "CARD123";

        // Act
        await Mediator.Publish(new CardRead(cardNumber));

        // Assert
        MifareServiceMock.Verify(
            x => x.StopSearchAsync(It.IsAny<CancellationToken>()),
            Times.Never,
            "Не должен обрабатывать чтение карты в неправильном состоянии");

        AssertState(EntryState.Idle);
        AssertCardNumber(null);
    }

    [Test]
    public async Task VehicleLeft_FromReadingCard_ShouldStopCardSearch()
    {
        // Arrange - начинаем в состоянии ReadingCard
        Setup(EntryState.ReadingCard);

        // Act
        await Mediator.Publish(new VehicleLeft());

        // Assert
        MifareServiceMock.Verify(
            x => x.StopSearchAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "Должен остановиться поиск карты когда автомобиль уехал");

        AssertState(EntryState.Idle);
    }
}
