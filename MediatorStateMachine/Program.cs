using MediatorStateMachine.HostedServices;
using MediatorStateMachine.StateMachine;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

builder.Services.AddSingleton<OrderStateMachine>();

builder.Services.AddHostedService<OrderCreationService>();
builder.Services.AddHostedService<OrderValidationService>();
builder.Services.AddHostedService<OrderProcessingService>();
builder.Services.AddHostedService<StatisticsService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

var host = builder.Build();
await host.RunAsync();
