// <copyright file="Program.cs" company="RPS">
// Copyright (c) RPS. All rights reserved.
// </copyright>

using ParkingEntry.Api.Services;
using ParkingEntry.Core.Application.Services;
using ParkingEntry.Core.Application.StateMachine;
using ParkingEntry.Core.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(ParkingEntry.Core.Domain.EntryState).Assembly);
});

// Register application services as Singletons (как указано в ТЗ)
builder.Services.AddSingleton<IStateContext, StateContext>();
builder.Services.AddSingleton<ITimeoutManager, TimeoutManager>();

// Register equipment services (заглушки для тестирования)
builder.Services.AddSingleton<IBarrierService, MockBarrierService>();
builder.Services.AddSingleton<ILightService, MockLightService>();
builder.Services.AddSingleton<IMifareService, MockMifareService>();
builder.Services.AddSingleton<IAccessCheckService, MockAccessCheckService>();
builder.Services.AddSingleton<IDebtCheckService, MockDebtCheckService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
