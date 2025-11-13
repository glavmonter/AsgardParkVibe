using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ParkingEntry.Application.Services.Commands;
using ParkingEntry.Application.Services.Handlers;
using ParkingEntry.Domain.Entities;
using ParkingEntry.Infrastructure.Repositories;
using ParkingEntry.Tests.Builders;

namespace ParkingEntry.Tests.Unit.Handlers;

/// <summary>
/// Unit tests for AccessCheckHandler.
/// </summary>
[TestFixture]
public class AccessCheckHandlerTests
{
    private Mock<IClientRepository> _repositoryMock = null!;
    private Mock<ILogger<AccessCheckHandler>> _loggerMock = null!;
    private AccessCheckHandler _sut = null!;

    [SetUp]
    public void Setup()
    {
        _repositoryMock = new Mock<IClientRepository>();
        _loggerMock = new Mock<ILogger<AccessCheckHandler>>();
        _sut = new AccessCheckHandler(_repositoryMock.Object, _loggerMock.Object);
    }

    [Test]
    public async Task Handle_ClientNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByNumberAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var command = new CheckAccessCommand("12345");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.Reason.Should().Be("Client not found");
    }

    [Test]
    public async Task Handle_ClientBlocked_ShouldReturnBlocked()
    {
        // Arrange
        var client = ClientBuilder.Create()
            .WithNumber("12345")
            .Blocked()
            .Build();

        _repositoryMock
            .Setup(r => r.GetByNumberAsync("12345", It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var command = new CheckAccessCommand("12345");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.Reason.Should().Be("Client is blocked");
    }

    [Test]
    public async Task Handle_ClientNoActiveContract_ShouldReturnNoContract()
    {
        // Arrange
        var client = ClientBuilder.Create()
            .WithNumber("12345")
            .WithoutActiveContract()
            .Build();

        _repositoryMock
            .Setup(r => r.GetByNumberAsync("12345", It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var command = new CheckAccessCommand("12345");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsAllowed.Should().BeFalse();
        result.Reason.Should().Be("No active contract");
    }

    [Test]
    public async Task Handle_ValidClient_ShouldReturnAllowed()
    {
        // Arrange
        var client = ClientBuilder.Create()
            .WithNumber("12345")
            .WithActiveContract()
            .NotBlocked()
            .Build();

        _repositoryMock
            .Setup(r => r.GetByNumberAsync("12345", It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        var command = new CheckAccessCommand("12345");

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.IsAllowed.Should().BeTrue();
        result.Reason.Should().BeEmpty();
    }

    [Test]
    public async Task Handle_ShouldCallRepository_WithCorrectCardNumber()
    {
        // Arrange
        var cardNumber = "TEST_CARD_123";
        var command = new CheckAccessCommand(cardNumber);

        _repositoryMock
            .Setup(r => r.GetByNumberAsync(cardNumber, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.GetByNumberAsync(cardNumber, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
