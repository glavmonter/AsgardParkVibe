using MediatR;
using Microsoft.AspNetCore.Mvc;
using ParkingGate.Domain.Queries;

namespace ParkingGate.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<StatusController> _logger;

    public StatusController(IMediator mediator, ILogger<StatusController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get current gate state
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        try
        {
            var state = await _mediator.Send(new GetGateStateQuery(), cancellationToken);
            return Ok(state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting gate status");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
