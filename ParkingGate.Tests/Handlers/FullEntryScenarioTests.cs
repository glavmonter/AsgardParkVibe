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
public class FullEntryScenarioTests
{
    private Mock<IMediator> _mediatorMock = null!;
    private IGateStateService _stateService = null!;
    private Mock<ILogger<VehicleApproachedEventHandler>> _approachedLoggerMock = null!;
    private Mock<ILogger<CardReadEventHandler>> _cardReadLoggerMock = null!;
    private Mock<ILogger<VehiclePassedThroughEventHandler>> _passedLoggerMock = null!;

    [SetUp]
    public void Setup()
    {
        _mediatorMock = new Mock<IMediator>();
        _stateService = new GateStateService();
        _approachedLoggerMock = new Mock<ILogger<VehicleApproachedEventHandler>>();
        _cardReadLoggerMock = new Mock<ILogger<CardReadEventHandler>>();
        _passedLoggerMock = new Mock<ILogger<VehiclePassedThroughEventHandler>>();
    }

    [Test]
    public async Task FullScenario_WithAccessGranted_ShouldCompleteSuccessfully()
    {
        // Arrange
        var approachedHandler = new VehicleApproachedEventHandler(
            _approachedLoggerMock.Object,
            _mediatorMock.Object,
            _stateService);

        var cardReadHandler = new CardReadEventHandler(
            _cardReadLoggerMock.Object,
            _mediatorMock.Object,
            _stateService);

        var passedHandler = new VehiclePassedThroughEventHandler(
            _passedLoggerMock.Object,
            _mediatorMock.Object,
            _stateService);

        // Setup mediator responses
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<SetLightColorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SetLightColorCommand cmd, CancellationToken _) => cmd.Color);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<StartSearchingCardCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MifareStatus.Searching);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<StopSearchingCardCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MifareStatus.Idle);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CheckAccessRightsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<OpenBarrierCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BarrierStatus.Open);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CloseBarrierCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BarrierStatus.Closed);

        // Act - Step 1: Vehicle approaches
        await approachedHandler.Handle(new VehicleApproachedEvent(), CancellationToken.None);

        // Assert Step 1
        _stateService.GetCurrentState().CurrentStateName.Should().Be("VehicleApproached");
        _mediatorMock.Verify(m => m.Send(
            It.Is<SetLightColorCommand>(cmd => cmd.Color == LightStatus.Red),
            It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(
            It.IsAny<StartSearchingCardCommand>(),
            It.IsAny<CancellationToken>()), Times.Once);

        // Act - Step 2: Card is read
        var card = new MifareCard("1234", DateTime.UtcNow);
        await cardReadHandler.Handle(new CardReadEvent { Card = card }, CancellationToken.None);

        // Assert Step 2
        _stateService.GetCurrentState().CurrentStateName.Should().Be("AccessGranted");
        _mediatorMock.Verify(m => m.Send(
            It.IsAny<StopSearchingCardCommand>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(
            It.IsAny<CheckAccessRightsCommand>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(
            It.IsAny<OpenBarrierCommand>(),
            It.IsAny<CancellationToken>()), Times.Once);

        // Act - Step 3: Vehicle passes through
        await passedHandler.Handle(new VehiclePassedThroughEvent(), CancellationToken.None);

        // Assert Step 3
        _stateService.GetCurrentState().CurrentStateName.Should().Be("WaitingForVehicle");
        _mediatorMock.Verify(m => m.Send(
            It.IsAny<CloseBarrierCommand>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(m => m.Send(
            It.Is<SetLightColorCommand>(cmd => cmd.Color == LightStatus.Green),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task CardRead_WithAccessDenied_ShouldNotOpenBarrier()
    {
        // Arrange
        var cardReadHandler = new CardReadEventHandler(
            _cardReadLoggerMock.Object,
            _mediatorMock.Object,
            _stateService);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<StopSearchingCardCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MifareStatus.Idle);

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<CheckAccessRightsCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var card = new MifareCard("9999", DateTime.UtcNow);
        await cardReadHandler.Handle(new CardReadEvent { Card = card }, CancellationToken.None);

        // Assert
        _stateService.GetCurrentState().CurrentStateName.Should().Be("AccessDenied");
        _mediatorMock.Verify(m => m.Send(
            It.IsAny<OpenBarrierCommand>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task StateTransitions_ShouldUpdateStateNameSequentially()
    {
        // Arrange
        var initialState = _stateService.GetCurrentState();
        initialState.CurrentStateName.Should().Be("WaitingForVehicle");

        // Setup handlers
        var approachedHandler = new VehicleApproachedEventHandler(
            _approachedLoggerMock.Object,
            _mediatorMock.Object,
            _stateService);

        var passedHandler = new VehiclePassedThroughEventHandler(
            _passedLoggerMock.Object,
            _mediatorMock.Object,
            _stateService);

        // Act & Assert - Vehicle approaches
        await approachedHandler.Handle(new VehicleApproachedEvent(), CancellationToken.None);
        _stateService.GetCurrentState().CurrentStateName.Should().Be("VehicleApproached");

        // Act & Assert - Vehicle passes through
        await passedHandler.Handle(new VehiclePassedThroughEvent(), CancellationToken.None);
        _stateService.GetCurrentState().CurrentStateName.Should().Be("WaitingForVehicle");
    }
}
