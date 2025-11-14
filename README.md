# MediatR State Machine Example

Пример приложения на C# с использованием паттерна Mediator (MediatR), тремя Hosted Services и конечным автоматом (State Machine) с пятью состояниями.

## Архитектура

### Конечный автомат (OrderStateMachine)

**5 состояний:**
1. **Created** - заказ создан
2. **Validated** - заказ провалидирован
3. **Processing** - заказ в обработке
4. **Completed** - заказ выполнен
5. **Failed** - заказ провален

**Переходы между состояниями:**
```
Created ---[ValidationSucceeded]---> Validated
Created ---[ValidationFailed]------> Failed
Validated ---[ProcessingStarted]---> Processing
Processing ---[ProcessingCompleted]-> Completed
Processing ---[ProcessingFailed]---> Failed
Failed ---[Reset]-------------------> Created
Completed ---[Reset]----------------> Created
```

### Hosted Services

1. **OrderCreationService** 
   - Создает новые заказы каждые 3-5 секунд
   - Отправляет команду `CreateOrderCommand` через MediatR

2. **OrderValidationService**
   - Отслеживает заказы в состоянии `Created`
   - Валидирует их (с вероятностью успеха 85%)
   - Отправляет команду `ValidateOrderCommand`

3. **OrderProcessingService**
   - Отслеживает заказы в состоянии `Validated`
   - Обрабатывает их (с вероятностью успеха 90%)
   - Отправляет команду `ProcessOrderCommand`

4. **StatisticsService** (бонус)
   - Выводит статистику по состояниям каждые 10 секунд

### MediatR команды и уведомления

**Команды (IRequest):**
- `CreateOrderCommand` - создание заказа
- `ValidateOrderCommand` - валидация заказа
- `ProcessOrderCommand` - обработка заказа

**Уведомления (INotification):**
- `OrderCreatedNotification` - заказ создан
- `OrderValidatedNotification` - заказ провалидирован
- `OrderProcessedNotification` - заказ обработан

## Структура проекта

```
MediatorStateMachine/
├── StateMachine/
│   ├── OrderStateDefinitions.cs    # Перечисления состояний и событий
│   └── OrderStateMachine.cs        # Конечный автомат (Singleton)
├── Commands/
│   ├── OrderCommands.cs            # Определения команд и уведомлений
│   ├── OrderCommandHandlers.cs     # Обработчики команд
│   └── OrderNotificationHandlers.cs # Обработчики уведомлений
├── HostedServices/
│   ├── OrderCreationService.cs     # Сервис создания заказов
│   ├── OrderValidationService.cs   # Сервис валидации
│   ├── OrderProcessingService.cs   # Сервис обработки
│   └── StatisticsService.cs        # Сервис статистики
└── Program.cs                      # Точка входа и регистрация сервисов

MediatorStateMachine.Tests/
├── OrderStateMachineTests.cs       # Тесты конечного автомата
└── CommandHandlersTests.cs         # Тесты обработчиков команд
```

## Запуск

### Требования
- .NET 8.0 SDK

### Восстановление зависимостей и сборка
```bash
dotnet restore
dotnet build
```

### Запуск приложения
```bash
dotnet run --project MediatorStateMachine
```

### Запуск тестов
```bash
dotnet test
```

## Пример вывода

```
📦 ORDER CREATED: ORD-0001 | Customer: Customer-42 | Amount: $156.78
✅ ORDER VALIDATED: ORD-0001 | Status: VALID
🎉 ORDER COMPLETED: ORD-0001 | Message: Order processed successfully

═══════════════════════════════════════
📊 STATE STATISTICS:
   🆕 Created: 2
   ✅ Validated: 1
   ⚙️  Processing: 0
   🎉 Completed: 5
   ❌ Failed: 3
   📦 TOTAL: 11
═══════════════════════════════════════
```

## Тесты

Тестовый проект включает:

### OrderStateMachineTests
- ✅ Создание заказов
- ✅ Переходы между всеми состояниями
- ✅ Проверка недопустимых переходов
- ✅ Полный жизненный цикл заказа
- ✅ Функция Reset
- ✅ Вспомогательные методы (CanTransition, GetOrdersInState, GetStateStatistics)
- ✅ Потокобезопасность
- ✅ Граничные случаи

### CommandHandlersTests
- ✅ Обработчики команд CreateOrder, ValidateOrder, ProcessOrder
- ✅ Публикация уведомлений через MediatR
- ✅ Проверка корректности состояний
- ✅ Интеграционные тесты полного цикла

Всего: **30+ тестов**

## Особенности реализации

1. **Thread-Safety**: OrderStateMachine использует `SemaphoreSlim` для потокобезопасности
2. **Singleton**: StateMachine зарегистрирован как Singleton для разделения состояния между сервисами
3. **Scoped Services**: Каждый HostedService создает scope для получения доступа к MediatR
4. **Async/Await**: Все операции асинхронные
5. **Structured Logging**: Используется ILogger с структурированными сообщениями
6. **CQRS Pattern**: Разделение команд (Commands) и запросов

## Технологии

- .NET 8.0
- MediatR 12.2.0
- Microsoft.Extensions.Hosting 8.0.0
- NUnit 4.0.1
- Moq 4.20.70

## Лицензия

MIT
