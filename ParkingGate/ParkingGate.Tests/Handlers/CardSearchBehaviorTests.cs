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
public class CardSearchBehaviorTests
{
    private Mock<IMediator> _mediatorMock = null!;
    private Mock<ILogger<VehicleApproachedEventHandler>> _loggerMock = null!;
    private IGateStateService _stateService = null!;

    [SetUp]
    public void Setup()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<VehicleApproachedEventHandler>>();
        _stateService = new GateStateService();
    }

    [Test]
    public async Task VehicleApproached_ShouldStartSearchingForCard()
    {
        // Arrange
        var handler = new VehicleApproachedEventHandler(
            _loggerMock.Object,
            _mediatorMock.Object,
            _stateService);

        var @event = new VehicleApproachedEvent();

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<StartSearchingCardCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MifareStatus.Searching);

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        _mediatorMock.Verify(
            m => m.Send(
                It.IsAny<StartSearchingCardCommand>(),
                It.IsAny<CancellationToken>()),
            Times.Once,
            "Card search should start when vehicle approaches");
    }

    [Test]
    public async Task VehicleApproached_ShouldUpdateStateName()
    {
        // Arrange
        var handler = new VehicleApproachedEventHandler(
            _loggerMock.Object,
            _mediatorMock.Object,
            _stateService);

        var @event = new VehicleApproachedEvent();

        // Act
        await handler.Handle(@event, CancellationToken.None);

        // Assert
        var state = _stateService.GetCurrentState();
        state.CurrentStateName.Should().Be("VehicleApproached");
    }
}
