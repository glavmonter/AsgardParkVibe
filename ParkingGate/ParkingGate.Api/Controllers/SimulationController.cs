using MediatR;
using Microsoft.AspNetCore.Mvc;
using ParkingGate.Domain.Events;
using ParkingGate.Domain.Models;

namespace ParkingGate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SimulationController : ControllerBase
{
    private readonly IPublisher _publisher;
    private readonly ILogger<SimulationController> _logger;

    public SimulationController(IPublisher publisher, ILogger<SimulationController> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    /// <summary>
    /// Simulate vehicle approaching the gate
    /// </summary>
    [HttpPost("vehicle/approached")]
    public async Task<IActionResult> VehicleApproached(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Simulating vehicle approached event");
            await _publisher.Publish(new VehicleApproachedEvent(), cancellationToken);
            return Ok(new { message = "Vehicle approached event published", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing vehicle approached event");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Simulate vehicle departing from the gate
    /// </summary>
    [HttpPost("vehicle/departed")]
    public async Task<IActionResult> VehicleDeparted(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Simulating vehicle departed event");
            await _publisher.Publish(new VehicleDepartedEvent(), cancellationToken);
            return Ok(new { message = "Vehicle departed event published", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing vehicle departed event");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Simulate vehicle passed through the gate
    /// </summary>
    [HttpPost("vehicle/passed-through")]
    public async Task<IActionResult> VehiclePassedThrough(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Simulating vehicle passed through event");
            await _publisher.Publish(new VehiclePassedThroughEvent(), cancellationToken);
            return Ok(new { message = "Vehicle passed through event published", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing vehicle passed through event");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Simulate vehicle backing out
    /// </summary>
    [HttpPost("vehicle/backed-out")]
    public async Task<IActionResult> VehicleBackedOut(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Simulating vehicle backed out event");
            await _publisher.Publish(new VehicleBackedOutEvent(), cancellationToken);
            return Ok(new { message = "Vehicle backed out event published", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing vehicle backed out event");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Simulate card read event (manual trigger for testing)
    /// </summary>
    [HttpPost("card/read")]
    public async Task<IActionResult> CardRead([FromBody] CardReadRequest request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Simulating card read event for card {CardNumber}", request.CardNumber);
            
            var card = new MifareCard(request.CardNumber, DateTime.UtcNow);
            await _publisher.Publish(new CardReadEvent { Card = card }, cancellationToken);
            
            return Ok(new { message = "Card read event published", cardNumber = request.CardNumber, timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing card read event");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

public record CardReadRequest(string CardNumber);
