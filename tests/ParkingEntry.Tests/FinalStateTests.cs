// <copyright file="FinalStateTests.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using FluentAssertions;
using Moq;
using NUnit.Framework;
using ParkingEntry.Core.Domain;
using ParkingEntry.Core.Domain.Events;

namespace ParkingEntry.Tests;

/// <summary>
/// Тесты завершающих фаз без прохождения всех начальных состояний.
/// </summary>
[TestFixture]
public class FinalStateTests : EntryStateMachineTestBase
{
    [Test]
    public async Task VehiclePassed_FromWaitingPassage_ShouldCompleteSuccessfully()
    {
        // Arrange - СРАЗУ начинаем с состояния WaitingPassage
        // Имитируем что автомобиль уже получил доступ и ждет проезда
        Setup(EntryState.WaitingPassage, cardNumber: "VALID_CARD_123");

        // Act - автомобиль проезжает
        await Mediator.Publish(new VehiclePassed());

        // Assert
        // 1. Должен закрыться шлагбаум
        BarrierServiceMock.Verify(
            x => x.CloseAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "Шлагбаум должен закрыться после проезда");

        // 2. Светофор должен стать зеленым
        LightServiceMock.Verify(
            x => x.SetColorAsync(LightColor.Green, It.IsAny<CancellationToken>()),
            Times.Once,
            "Светофор должен стать зеленым");

        // 3. Система должна вернуться в Idle
        AssertState(EntryState.Idle);

        // 4. Номер карты должен быть сброшен
        AssertCardNumber(null);
    }

    [Test]
    public async Task VehicleReversed_FromWaitingPassage_ShouldCompleteSuccessfully()
    {
        // Arrange - СРАЗУ начинаем с состояния WaitingPassage
        Setup(EntryState.WaitingPassage, cardNumber: "ANOTHER_CARD_456");

        // Act - автомобиль уехал назад
        await Mediator.Publish(new VehicleReversed());

        // Assert
        // 1. Должен закрыться шлагбаум
        BarrierServiceMock.Verify(
            x => x.CloseAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "Шлагбаум должен закрыться когда автомобиль уехал назад");

        // 2. Светофор должен стать зеленым
        LightServiceMock.Verify(
            x => x.SetColorAsync(LightColor.Green, It.IsAny<CancellationToken>()),
            Times.Once,
            "Светофор должен стать зеленым");

        // 3. Система должна вернуться в Idle
        AssertState(EntryState.Idle);

        // 4. Номер карты должен быть сброшен
        AssertCardNumber(null);
    }

    [Test]
    public async Task VehicleLeft_FromAccessDenied_ShouldReturnToIdle()
    {
        // Arrange - СРАЗУ начинаем с состояния AccessDenied
        // Имитируем что карта была отклонена
        Setup(EntryState.AccessDenied, cardNumber: "DENIED_CARD");

        // Act - автомобиль уезжает
        await Mediator.Publish(new VehicleLeft());

        // Assert
        // 1. Светофор должен стать зеленым
        LightServiceMock.Verify(
            x => x.SetColorAsync(LightColor.Green, It.IsAny<CancellationToken>()),
            Times.Once,
            "Светофор должен стать зеленым");

        // 2. Система должна вернуться в Idle
        AssertState(EntryState.Idle);

        // 3. Номер карты должен быть сброшен
        AssertCardNumber(null);
    }

    [Test]
    public async Task ClosingBarrier_AfterVehiclePassed_ShouldTransitionCorrectly()
    {
        // Arrange - начинаем с WaitingPassage
        Setup(EntryState.WaitingPassage, cardNumber: "TEST_CARD");

        // Act
        await Mediator.Publish(new VehiclePassed());

        // Assert - проверяем последовательность вызовов
        var invocations = BarrierServiceMock.Invocations
            .Where(i => i.Method.Name == nameof(IBarrierService.CloseAsync))
            .ToList();

        invocations.Should().HaveCount(1, "Шлагбаум должен быть закрыт ровно один раз");

        // Финальное состояние - Idle
        AssertState(EntryState.Idle);
    }

    [Test]
    public async Task MultipleVehiclePassed_FromWaitingPassage_OnlyFirstShouldProcess()
    {
        // Arrange - начинаем с WaitingPassage
        Setup(EntryState.WaitingPassage, cardNumber: "CARD999");

        // Act - отправляем событие дважды
        await Mediator.Publish(new VehiclePassed());
        await Mediator.Publish(new VehiclePassed());

        // Assert - шлагбаум должен быть закрыт только один раз
        // (второе событие должно быть проигнорировано, так как состояние уже Idle)
        BarrierServiceMock.Verify(
            x => x.CloseAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "Шлагбаум должен быть закрыт только один раз");

        AssertState(EntryState.Idle);
    }

    [Test]
    public async Task VehiclePassed_WithoutCardNumber_ShouldStillComplete()
    {
        // Arrange - начинаем с WaitingPassage БЕЗ номера карты
        // (необычный сценарий, но система должна быть устойчива)
        Setup(EntryState.WaitingPassage, cardNumber: null);

        // Act
        await Mediator.Publish(new VehiclePassed());

        // Assert - операция должна завершиться нормально
        BarrierServiceMock.Verify(
            x => x.CloseAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        AssertState(EntryState.Idle);
    }

    [Test]
    public async Task StateTransitions_FromWaitingToIdle_ShouldCleanUpCorrectly()
    {
        // Arrange
        Setup(EntryState.WaitingPassage, cardNumber: "CLEANUP_TEST");

        // Act
        await Mediator.Publish(new VehicleReversed());

        // Assert - проверяем что все очищено
        StateContext.CurrentState.Should().Be(EntryState.Idle, "Состояние должно быть Idle");
        StateContext.CurrentCardNumber.Should().BeNull("Номер карты должен быть очищен");

        // Можем снова обрабатывать новый автомобиль
        await Mediator.Publish(new VehicleApproached());
        AssertState(EntryState.ReadingCard);
    }
}
