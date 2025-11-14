// <copyright file="EntryController.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using MediatR;
using Microsoft.AspNetCore.Mvc;
using ParkingEntry.Core.Application.StateMachine;
using ParkingEntry.Core.Domain.Events;

namespace ParkingEntry.Api.Controllers;

/// <summary>
/// Контроллер для управления въездной стойкой.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EntryController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IStateContext _stateContext;
    private readonly ILogger<EntryController> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="EntryController"/>.
    /// </summary>
    public EntryController(
        IMediator mediator,
        IStateContext stateContext,
        ILogger<EntryController> logger)
    {
        _mediator = mediator;
        _stateContext = stateContext;
        _logger = logger;
    }

    /// <summary>
    /// Получить текущий статус системы.
    /// </summary>
    /// <returns>Статус.</returns>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            CurrentState = _stateContext.CurrentState.ToString(),
            CardNumber = _stateContext.CurrentCardNumber,
            Timestamp = DateTime.UtcNow,
        });
    }

    /// <summary>
    /// Отправить событие "Автомобиль подъехал".
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат.</returns>
    [HttpPost("events/vehicle-approached")]
    public async Task<IActionResult> VehicleApproached(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Получено событие: VehicleApproached");
        await _mediator.Publish(new VehicleApproached(), cancellationToken);
        return Ok(new { Message = "Event published", State = _stateContext.CurrentState.ToString() });
    }

    /// <summary>
    /// Отправить событие "Автомобиль уехал".
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат.</returns>
    [HttpPost("events/vehicle-left")]
    public async Task<IActionResult> VehicleLeft(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Получено событие: VehicleLeft");
        await _mediator.Publish(new VehicleLeft(), cancellationToken);
        return Ok(new { Message = "Event published", State = _stateContext.CurrentState.ToString() });
    }

    /// <summary>
    /// Отправить событие "Автомобиль проехал".
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат.</returns>
    [HttpPost("events/vehicle-passed")]
    public async Task<IActionResult> VehiclePassed(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Получено событие: VehiclePassed");
        await _mediator.Publish(new VehiclePassed(), cancellationToken);
        return Ok(new { Message = "Event published", State = _stateContext.CurrentState.ToString() });
    }

    /// <summary>
    /// Отправить событие "Автомобиль уехал назад".
    /// </summary>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат.</returns>
    [HttpPost("events/vehicle-reversed")]
    public async Task<IActionResult> VehicleReversed(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Получено событие: VehicleReversed");
        await _mediator.Publish(new VehicleReversed(), cancellationToken);
        return Ok(new { Message = "Event published", State = _stateContext.CurrentState.ToString() });
    }

    /// <summary>
    /// Отправить событие "Карта прочитана".
    /// </summary>
    /// <param name="request">Данные карты.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>Результат.</returns>
    [HttpPost("events/card-read")]
    public async Task<IActionResult> CardRead([FromBody] CardReadRequest request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Получено событие: CardRead с номером {CardNumber}", request.CardNumber);
        await _mediator.Publish(new CardRead(request.CardNumber), cancellationToken);
        return Ok(new { Message = "Event published", State = _stateContext.CurrentState.ToString() });
    }

    /// <summary>
    /// Сбросить систему в начальное состояние.
    /// </summary>
    /// <returns>Результат.</returns>
    [HttpPost("reset")]
    public IActionResult Reset()
    {
        _logger.LogInformation("Сброс системы");
        _stateContext.Reset();
        return Ok(new { Message = "System reset", State = _stateContext.CurrentState.ToString() });
    }
}

/// <summary>
/// Запрос на чтение карты.
/// </summary>
/// <param name="CardNumber">Номер карты.</param>
public record CardReadRequest(string CardNumber);
