using FluentAssertions;
using Mediator;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ParkingEntry.Application.Services.Commands;
using ParkingEntry.Application.StateMachine;
using ParkingEntry.Domain.Enums;
using ParkingEntry.Domain.Events;

namespace ParkingEntry.Tests.Unit.StateMachine;

/// <summary>
/// Unit tests for EntryStateMachine.
/// </summary>
[TestFixture]
public class EntryStateMachineTests
{
    private Mock<IMediator> _mediatorMock = null!;
    private Mock<ILogger<EntryStateMachine>> _loggerMock = null!;
    private EntryStateMachine _sut = null!;

    [SetUp]
    public void Setup()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<EntryStateMachine>>();
        _sut = new EntryStateMachine(_mediatorMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task TransitionAsync_FromIdle_WithVehicleApproached_ShouldTransitionToReadingCard()
    {
        // Arrange
        var vehicleApproached = new VehicleApproached();

        // Act
        var newState = await _sut.TransitionAsync(vehicleApproached, CancellationToken.None);

        // Assert
        newState.Should().Be(EntryState.ReadingCard);
        _sut.CurrentState.Should().Be(EntryState.ReadingCard);

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<SwitchLightCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<StartCardSearchCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task TransitionAsync_FromReadingCard_WithCardRead_ShouldTransitionToCheckingAccess()
    {
        // Arrange
        await _sut.TransitionToAsync(EntryState.ReadingCard, CancellationToken.None);
        _mediatorMock.ResetCalls();

        var cardRead = new CardRead("12345");

        // Act
        var newState = await _sut.TransitionAsync(cardRead, CancellationToken.None);

        // Assert
        newState.Should().Be(EntryState.CheckingAccess);
        _sut.CurrentState.Should().Be(EntryState.CheckingAccess);
    }

    [Test]
    public async Task TransitionAsync_FromWaitingPassage_WithVehiclePassed_ShouldTransitionToIdle()
    {
        // Arrange
        await _sut.TransitionToAsync(EntryState.WaitingPassage, CancellationToken.None);
        _mediatorMock.ResetCalls();

        var vehiclePassed = new VehiclePassed();

        // Act
        var newState = await _sut.TransitionAsync(vehiclePassed, CancellationToken.None);

        // Assert
        newState.Should().Be(EntryState.Idle);
        _sut.CurrentState.Should().Be(EntryState.Idle);

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<CloseBarrierCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task EnterState_ReadingCard_ShouldSwitchLightRedAndStartCardSearch()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SwitchLightCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LightStatus(LightColor.Red));

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<StartCardSearchCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MifareStatus.Searching);

        // Act
        await _sut.TransitionToAsync(EntryState.ReadingCard, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<SwitchLightCommand>(cmd => cmd.Color == LightColor.Red),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _mediatorMock.Verify(
            m => m.Send(It.IsAny<StartCardSearchCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task ExitState_ReadingCard_ShouldStopCardSearch()
    {
        // Arrange
        await _sut.TransitionToAsync(EntryState.ReadingCard, CancellationToken.None);
        _mediatorMock.ResetCalls();

        // Act
        await _sut.TransitionToAsync(EntryState.Idle, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<StopCardSearchCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task EnterState_OpeningBarrier_ShouldOpenBarrier()
    {
        // Arrange
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<OpenBarrierCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BarrierStatus.Open);

        // Act
        await _sut.TransitionToAsync(EntryState.OpeningBarrier, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(It.IsAny<OpenBarrierCommand>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void CurrentState_InitialState_ShouldBeIdle()
    {
        // Assert
        _sut.CurrentState.Should().Be(EntryState.Idle);
    }
}
