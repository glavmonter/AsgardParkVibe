// <copyright file="BusinessCommandHandlers.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using MediatR;
using ParkingEntry.Core.Application.Commands;
using ParkingEntry.Core.Application.Services;

namespace ParkingEntry.Core.Application.Handlers;

/// <summary>
/// Обработчик команды проверки доступа.
/// </summary>
public sealed class CheckAccessCommandHandler : IRequestHandler<CheckAccessCommand, AccessCheckResult>
{
    private readonly IAccessCheckService _accessCheckService;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CheckAccessCommandHandler"/>.
    /// </summary>
    public CheckAccessCommandHandler(IAccessCheckService accessCheckService)
    {
        _accessCheckService = accessCheckService;
    }

    /// <inheritdoc/>
    public async Task<AccessCheckResult> Handle(CheckAccessCommand request, CancellationToken cancellationToken)
    {
        var (isAllowed, reason) = await _accessCheckService.CheckAccessAsync(
            request.ClientNumber,
            cancellationToken);

        return new AccessCheckResult(isAllowed, reason);
    }
}

/// <summary>
/// Обработчик команды проверки задолженности.
/// </summary>
public sealed class CheckDebtCommandHandler : IRequestHandler<CheckDebtCommand, decimal>
{
    private readonly IDebtCheckService _debtCheckService;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CheckDebtCommandHandler"/>.
    /// </summary>
    public CheckDebtCommandHandler(IDebtCheckService debtCheckService)
    {
        _debtCheckService = debtCheckService;
    }

    /// <inheritdoc/>
    public Task<decimal> Handle(CheckDebtCommand request, CancellationToken cancellationToken)
    {
        return _debtCheckService.CalculateDebtAsync(request.ClientNumber, cancellationToken);
    }
}
