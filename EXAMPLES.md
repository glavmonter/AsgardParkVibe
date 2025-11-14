# Примеры использования

## Как использовать OrderStateMachine

### Пример 1: Создание заказа и переход по состояниям

```csharp
// Получение StateMachine из DI
var stateMachine = serviceProvider.GetRequiredService<OrderStateMachine>();

// Создание нового заказа
var orderId = "ORD-12345";
bool created = await stateMachine.CreateOrderAsync(orderId);

if (created)
{
    Console.WriteLine($"Order {orderId} created");
    
    // Проверка текущего состояния
    var currentState = stateMachine.GetOrderState(orderId);
    Console.WriteLine($"Current state: {currentState}"); // Created
    
    // Переход к следующему состоянию
    bool transitioned = await stateMachine.TriggerEventAsync(
        orderId, 
        OrderEvent.ValidationSucceeded);
    
    if (transitioned)
    {
        currentState = stateMachine.GetOrderState(orderId);
        Console.WriteLine($"New state: {currentState}"); // Validated
    }
}
```

### Пример 2: Проверка возможности перехода

```csharp
var orderId = "ORD-12345";
var currentState = stateMachine.GetOrderState(orderId);

// Проверяем, можно ли перейти в следующее состояние
if (stateMachine.CanTransition(orderId, OrderEvent.ProcessingStarted))
{
    Console.WriteLine("Can start processing");
    await stateMachine.TriggerEventAsync(orderId, OrderEvent.ProcessingStarted);
}
else
{
    Console.WriteLine($"Cannot start processing from state {currentState}");
}
```

### Пример 3: Получение списка заказов по состоянию

```csharp
// Получить все заказы, ожидающие валидации
var ordersToValidate = stateMachine.GetOrdersInState(OrderState.Created);

foreach (var orderId in ordersToValidate)
{
    Console.WriteLine($"Order {orderId} needs validation");
    // Обработка заказа...
}

// Получить все завершенные заказы
var completedOrders = stateMachine.GetOrdersInState(OrderState.Completed);
Console.WriteLine($"Completed orders: {completedOrders.Count()}");
```

### Пример 4: Получение статистики

```csharp
var statistics = stateMachine.GetStateStatistics();

Console.WriteLine("Order Statistics:");
foreach (var (state, count) in statistics)
{
    Console.WriteLine($"  {state}: {count}");
}

// Вывод:
// Order Statistics:
//   Created: 5
//   Validated: 3
//   Processing: 2
//   Completed: 15
//   Failed: 1
```

## Использование MediatR команд

### Пример 5: Отправка команды на создание заказа

```csharp
var mediator = serviceProvider.GetRequiredService<IMediator>();

var command = new CreateOrderCommand(
    OrderId: "ORD-12345",
    CustomerName: "John Doe",
    Amount: 199.99m
);

bool result = await mediator.Send(command);

if (result)
{
    Console.WriteLine("Order created successfully");
}
```

### Пример 6: Подписка на уведомления

```csharp
// Создаем обработчик уведомлений
public class CustomOrderCreatedHandler : INotificationHandler<OrderCreatedNotification>
{
    private readonly ILogger<CustomOrderCreatedHandler> _logger;
    
    public CustomOrderCreatedHandler(ILogger<CustomOrderCreatedHandler> logger)
    {
        _logger = logger;
    }
    
    public Task Handle(OrderCreatedNotification notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "New order created: {OrderId} for {Customer} - ${Amount}",
            notification.OrderId,
            notification.CustomerName,
            notification.Amount);
        
        // Можно отправить email, записать в базу данных и т.д.
        
        return Task.CompletedTask;
    }
}

// Регистрация в Program.cs
builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
```

## Создание собственного Hosted Service

### Пример 7: Кастомный Hosted Service

```csharp
public class CustomMonitoringService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CustomMonitoringService> _logger;

    public CustomMonitoringService(
        IServiceProvider serviceProvider,
        ILogger<CustomMonitoringService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("CustomMonitoringService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var stateMachine = scope.ServiceProvider.GetRequiredService<OrderStateMachine>();

                // Мониторинг заказов в состоянии Failed
                var failedOrders = stateMachine.GetOrdersInState(OrderState.Failed);
                
                if (failedOrders.Any())
                {
                    _logger.LogWarning(
                        "Found {Count} failed orders: {OrderIds}",
                        failedOrders.Count(),
                        string.Join(", ", failedOrders));
                    
                    // Здесь можно отправить алерт или попытаться переобработать
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CustomMonitoringService");
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        _logger.LogInformation("CustomMonitoringService stopped");
    }
}

// Регистрация
builder.Services.AddHostedService<CustomMonitoringService>();
```

## Тестирование

### Пример 8: Юнит-тест для StateMachine

```csharp
[Test]
public async Task CustomScenario_ValidationFailed_ThenReset_ShouldWork()
{
    // Arrange
    var orderId = "TEST-001";
    await _stateMachine.CreateOrderAsync(orderId);

    // Act - Проваливаем валидацию
    await _stateMachine.TriggerEventAsync(orderId, OrderEvent.ValidationFailed);
    var failedState = _stateMachine.GetOrderState(orderId);

    // Сбрасываем заказ
    await _stateMachine.TriggerEventAsync(orderId, OrderEvent.Reset);
    var resetState = _stateMachine.GetOrderState(orderId);

    // Пытаемся снова
    await _stateMachine.TriggerEventAsync(orderId, OrderEvent.ValidationSucceeded);
    var validatedState = _stateMachine.GetOrderState(orderId);

    // Assert
    Assert.That(failedState, Is.EqualTo(OrderState.Failed));
    Assert.That(resetState, Is.EqualTo(OrderState.Created));
    Assert.That(validatedState, Is.EqualTo(OrderState.Validated));
}
```

### Пример 9: Интеграционный тест

```csharp
[Test]
public async Task IntegrationTest_FullOrderLifecycle()
{
    // Arrange
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddMediatR(cfg => 
        cfg.RegisterServicesFromAssembly(typeof(OrderStateMachine).Assembly));
    services.AddSingleton<OrderStateMachine>();
    
    var provider = services.BuildServiceProvider();
    var mediator = provider.GetRequiredService<IMediator>();

    // Act - Создаем заказ
    var createCmd = new CreateOrderCommand("TEST-001", "John Doe", 199.99m);
    var created = await mediator.Send(createCmd);

    // Валидируем
    var validateCmd = new ValidateOrderCommand("TEST-001");
    bool validated = false;
    
    // Повторяем до успеха (из-за случайности)
    for (int i = 0; i < 10 && !validated; i++)
    {
        validated = await mediator.Send(validateCmd);
        var sm = provider.GetRequiredService<OrderStateMachine>();
        if (sm.GetOrderState("TEST-001") != OrderState.Validated)
        {
            await sm.TriggerEventAsync("TEST-001", OrderEvent.Reset);
            await mediator.Send(createCmd);
        }
    }

    // Assert
    Assert.That(created, Is.True);
    Assert.That(validated, Is.True);
}
```

## Расширение функциональности

### Пример 10: Добавление новых команд

```csharp
// Новая команда для отмены заказа
public record CancelOrderCommand(string OrderId, string Reason) : IRequest<bool>;

// Обработчик
public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, bool>
{
    private readonly OrderStateMachine _stateMachine;
    private readonly IMediator _mediator;
    private readonly ILogger<CancelOrderCommandHandler> _logger;

    public CancelOrderCommandHandler(
        OrderStateMachine stateMachine,
        IMediator mediator,
        ILogger<CancelOrderCommandHandler> logger)
    {
        _stateMachine = stateMachine;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<bool> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling order {OrderId}: {Reason}", 
            request.OrderId, request.Reason);

        var currentState = _stateMachine.GetOrderState(request.OrderId);
        
        // Можно отменить только заказы в определенных состояниях
        if (currentState is OrderState.Created or OrderState.Validated)
        {
            var result = await _stateMachine.TriggerEventAsync(
                request.OrderId, 
                OrderEvent.ValidationFailed); // Используем существующее событие
            
            if (result)
            {
                await _mediator.Publish(
                    new OrderCancelledNotification(request.OrderId, request.Reason),
                    cancellationToken);
            }
            
            return result;
        }

        _logger.LogWarning("Cannot cancel order {OrderId} in state {State}", 
            request.OrderId, currentState);
        return false;
    }
}

// Уведомление
public record OrderCancelledNotification(string OrderId, string Reason) : INotification;
```

## Лучшие практики

1. **Всегда используйте Scoped сервисы в HostedServices**
   ```csharp
   using var scope = _serviceProvider.CreateScope();
   var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
   ```

2. **Обрабатывайте исключения в BackgroundService**
   ```csharp
   try
   {
       // Ваш код
   }
   catch (OperationCanceledException)
   {
       // Нормальная остановка
       break;
   }
   catch (Exception ex)
   {
       _logger.LogError(ex, "Error occurred");
       await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
   }
   ```

3. **Проверяйте CancellationToken**
   ```csharp
   while (!stoppingToken.IsCancellationRequested)
   {
       // Работа
   }
   ```

4. **Используйте структурированное логирование**
   ```csharp
   _logger.LogInformation(
       "Order {OrderId} processed for {Customer}",
       orderId,
       customerName);
   ```
