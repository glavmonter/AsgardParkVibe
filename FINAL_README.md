# C# MediatR State Machine - Полный пример

## Описание

Полноценный пример C# приложения на .NET 8 с использованием:
- **MediatR** - паттерн Mediator для команд и уведомлений
- **3 Hosted Services** - фоновые сервисы для создания, валидации и обработки заказов
- **Конечный автомат (Singleton)** - управление состояниями с 5 состояниями и событийными переходами
- **NUnit тесты** - 30+ тестов с проверкой всех аспектов работы

## Структура проекта

```
MediatorStateMachine/
├── MediatorStateMachine.sln                    # Solution файл
├── README.md                                    # Основная документация
├── DIAGRAMS.md                                  # Диаграммы состояний и взаимодействий
├── EXAMPLES.md                                  # Примеры использования
├── TESTS.md                                     # Документация по тестам
│
├── MediatorStateMachine/                        # Основной проект
│   ├── MediatorStateMachine.csproj
│   ├── Program.cs
│   │
│   ├── StateMachine/
│   │   ├── OrderStateDefinitions.cs           # Перечисления состояний и событий
│   │   └── OrderStateMachine.cs               # Конечный автомат (Singleton)
│   │
│   ├── Commands/
│   │   ├── OrderCommands.cs                   # Команды и уведомления
│   │   ├── OrderCommandHandlers.cs            # Обработчики команд
│   │   └── OrderNotificationHandlers.cs       # Обработчики уведомлений
│   │
│   └── HostedServices/
│       ├── OrderCreationService.cs            # Сервис создания заказов
│       ├── OrderValidationService.cs          # Сервис валидации
│       ├── OrderProcessingService.cs          # Сервис обработки
│       └── StatisticsService.cs               # Сервис статистики
│
└── MediatorStateMachine.Tests/                 # Тестовый проект
    ├── MediatorStateMachine.Tests.csproj
    ├── OrderStateMachineTests.cs              # Тесты конечного автомата
    └── CommandHandlersTests.cs                # Тесты обработчиков команд
```

## Быстрый старт

### 1. Требования
- .NET 8.0 SDK

### 2. Сборка проекта

```bash
# Клонировать репозиторий или скачать файлы
cd MediatorStateMachine

# Восстановить зависимости
dotnet restore

# Собрать проект
dotnet build
```

### 3. Запуск приложения

```bash
dotnet run --project MediatorStateMachine
```

### 4. Запуск тестов

```bash
# Запустить все тесты
dotnet test

# С подробным выводом
dotnet test --logger "console;verbosity=detailed"

# С фильтрацией
dotnet test --filter "FullyQualifiedName~OrderStateMachineTests"
```

## Архитектура

### Конечный автомат

**5 состояний:**
1. **Created** - заказ создан
2. **Validated** - заказ провалидирован  
3. **Processing** - заказ в обработке
4. **Completed** - заказ выполнен
5. **Failed** - заказ провален

**Переходы между состояниями:**
```
Created → Validated       [ValidationSucceeded]
Created → Failed          [ValidationFailed]
Validated → Processing    [ProcessingStarted]
Processing → Completed    [ProcessingCompleted]
Processing → Failed       [ProcessingFailed]
Failed → Created          [Reset]
Completed → Created       [Reset]
```

### Hosted Services

1. **OrderCreationService** - создает новые заказы каждые 3-5 секунд
2. **OrderValidationService** - валидирует заказы в состоянии `Created`
3. **OrderProcessingService** - обрабатывает заказы в состоянии `Validated`
4. **StatisticsService** - выводит статистику каждые 10 секунд

### MediatR

**Команды:**
- `CreateOrderCommand` - создание заказа
- `ValidateOrderCommand` - валидация заказа
- `ProcessOrderCommand` - обработка заказа

**Уведомления:**
- `OrderCreatedNotification` - заказ создан
- `OrderValidatedNotification` - заказ провалидирован
- `OrderProcessedNotification` - заказ обработан

## Особенности реализации

✅ **Thread-Safety** - использование `SemaphoreSlim` для потокобезопасности  
✅ **Singleton Pattern** - единый экземпляр State Machine для всех сервисов  
✅ **CQRS** - разделение команд и запросов  
✅ **Async/Await** - полностью асинхронная архитектура  
✅ **Structured Logging** - логирование с структурированными данными  
✅ **Dependency Injection** - использование встроенного DI контейнера  

## Тестовое покрытие

- **30+ тестов** NUnit
- **95%+ покрытие кода**
- **100% публичных методов** покрыты тестами
- **Потокобезопасность** проверена
- **Граничные случаи** включены

### Категории тестов

**OrderStateMachineTests (22 теста):**
- Создание заказов
- Переходы между состояниями
- Полный жизненный цикл
- Функция Reset
- Вспомогательные методы
- Потокобезопасность
- Граничные случаи

**CommandHandlersTests (8 тестов):**
- Обработчики команд
- Публикация уведомлений
- Интеграционные тесты

## Зависимости

```xml
<PackageReference Include="MediatR" Version="12.2.0" />
<PackageReference Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="NUnit" Version="4.0.1" />
<PackageReference Include="Moq" Version="4.20.70" />
```

## Пример вывода

```
info: MediatorStateMachine.Commands.OrderCreatedNotificationHandler[0]
      📦 ORDER CREATED: ORD-0001 | Customer: Customer-42 | Amount: $156.78
      
info: MediatorStateMachine.Commands.OrderValidatedNotificationHandler[0]
      ✅ ORDER VALIDATED: ORD-0001 | Status: VALID
      
info: MediatorStateMachine.Commands.OrderProcessedNotificationHandler[0]
      🎉 ORDER COMPLETED: ORD-0001 | Message: Order processed successfully

info: MediatorStateMachine.HostedServices.StatisticsService[0]
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

## Дополнительная документация

- **[DIAGRAMS.md](DIAGRAMS.md)** - диаграммы состояний и последовательности действий
- **[EXAMPLES.md](EXAMPLES.md)** - примеры использования API
- **[TESTS.md](TESTS.md)** - подробная документация по тестам

## Технологии

- **.NET 8.0** - платформа разработки
- **MediatR 12.2.0** - реализация паттерна Mediator
- **Microsoft.Extensions.Hosting** - поддержка Hosted Services
- **NUnit 4.0.1** - фреймворк для unit-тестов
- **Moq 4.20.70** - библиотека для создания моков

## Лицензия

MIT License

## Автор

Пример создан для демонстрации архитектурных паттернов в C#:
- Паттерн Mediator (MediatR)
- Конечный автомат (State Machine)
- Hosted Services (Background Services)
- CQRS (Command Query Responsibility Segregation)
- Dependency Injection
- Unit Testing
