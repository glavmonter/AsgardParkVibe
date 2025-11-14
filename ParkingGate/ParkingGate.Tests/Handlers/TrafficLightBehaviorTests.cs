using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ParkingGate.Application.Handlers.Events;
using ParkingGate.Domain.Commands;
using ParkingGate.Domain.Events;
using ParkingGate.Domain.Models;
using ParkingGate.Infrastructure.Services;

namespace ParkingGate.Tests.Handlers;

[TestFixture]
public class TrafficLightBehaviorTests
{
    private Mock<IMediator> _mediatorMock = null!;
    private Mock<ILogger<VehicleApproachedEventHandler>> _approachedLoggerMock = null!;
    private Mock<ILogger<VehicleDepartedEventHandler>> _departedLoggerMock = null!;
    private Mock<ILogger<VehiclePassedThroughEventHandler>> _passedLoggerMock = null!;
    private Mock<ILogger<VehicleBackedOutEventHandler>> _backedLoggerMock = null!;
    private IGateStateService _stateService = null!;

    [SetUp]
    public void Setup()
    {
        _mediatorMock = new Mock<IMediator>();
        _approachedLoggerMock = new Mock<ILogger<VehicleApproachedEventHandler>>();
        _departedLoggerMock = new Mock<ILogger<VehicleDepartedEventHandler>>();
        _passedLoggerMock = new Mock<ILogger<VehiclePassedThroughEventHandler>>();
        _backedLoggerMock = new Mock<ILogger<VehicleBackedOutEventHandler>>();
        _stateService = new GateStateService();
    }

    [Test]
    public async Task VehicleApproached_ShouldSetLightToRed()
    {
        // Arrange
        var handler = new VehicleApproachedEventHandler(
            _approachedLoggerMock.Object,
            _mediatorMock.Object,
            _stateService);

        var @event = new VehicleApproachedEvent();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SetLightColorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(LightStatus.Red);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<SetLightColorCommand>(cmd => cmd.Color == LightStatus.Red),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task VehicleDeparted_ShouldSetLightToGreen()
    {
        // Arrange
        var handler = new VehicleDepartedEventHandler(
            _departedLoggerMock.Object,
            _mediatorMock.Object,
            _stateService);

        var @event = new VehicleDepartedEvent();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SetLightColorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(LightStatus.Green);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<SetLightColorCommand>(cmd => cmd.Color == LightStatus.Green),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task VehiclePassedThrough_ShouldSetLightToGreen()
    {
        // Arrange
        var handler = new VehiclePassedThroughEventHandler(
            _passedLoggerMock.Object,
            _mediatorMock.Object,
            _stateService);

        var @event = new VehiclePassedThroughEvent();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SetLightColorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(LightStatus.Green);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<SetLightColorCommand>(cmd => cmd.Color == LightStatus.Green),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public async Task VehicleBackedOut_ShouldSetLightToGreen()
    {
        // Arrange
        var handler = new VehicleBackedOutEventHandler(
            _backedLoggerMock.Object,
            _mediatorMock.Object,
            _stateService);

        var @event = new VehicleBackedOutEvent();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SetLightColorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(LightStatus.Green);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.Is<SetLightColorCommand>(cmd => cmd.Color == LightStatus.Green),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
