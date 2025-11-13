# Parking Entry System

Система управления въездной стойкой парковки на базе C# и ASP.NET с использованием паттернов FSM (Finite State Machine) и Mediator.

## Архитектура

Проект использует гибридную архитектуру:
- **FSM (State Machine)** - для управления состояниями въезда
- **Mediator** - для слабой связанности между компонентами
- **Clean Architecture** - разделение на слои Domain, Application, Infrastructure

### Структура проекта

```
ParkingEntry/
├── ParkingEntry.Domain/          # Доменные сущности и события
│   ├── Entities/                 # Сущности (Client, EntrySession, Card)
│   ├── Enums/                    # Перечисления состояний
│   └── Events/                   # Доменные события
│
├── ParkingEntry.Application/     # Бизнес-логика
│   ├── StateMachine/             # Конечный автомат состояний
│   ├── Services/                 # Команды и обработчики (Mediator)
│   ├── Coordinators/             # Координаторы процессов
│   └── Infrastructure/           # Инфраструктура приложения
│
├── ParkingEntry.Infrastructure/  # Внешние зависимости
│   ├── Hardware/                 # Сервисы оборудования
│   ├── Workers/                  # Фоновые задачи
│   └── Repositories/             # Репозитории данных
│
└── ParkingEntry.Tests/           # Тесты
    ├── Unit/                     # Модульные тесты
    ├── Integration/              # Интеграционные тесты
    └── Builders/                 # Test builders
```

## Компоненты системы

### 1. State Machine (FSM)

Управляет состояниями въездного процесса:

```
Idle → ReadingCard → CheckingAccess → OpeningBarrier → WaitingPassage → Idle
```

**Состояния:**
- `Idle` - Ожидание автомобиля
- `ReadingCard` - Чтение карты Mifare
- `CheckingAccess` - Проверка прав доступа
- `OpeningBarrier` - Открытие шлагбаума
- `WaitingPassage` - Ожидание проезда
- `Error` - Ошибка оборудования

### 2. Mediator Pattern

Используется библиотека `Mediator` для:
- Изоляции команд оборудования
- Слабой связанности компонентов
- Упрощения тестирования

**Команды:**
- `OpenBarrierCommand` / `CloseBarrierCommand`
- `SwitchLightCommand`
- `StartCardSearchCommand` / `StopCardSearchCommand`
- `CheckAccessCommand`
- `CalculateDebtCommand`

### 3. Hardware Services

**IBarrierService** - Управление шлагбаумом
```csharp
Task<BarrierStatus> OpenAsync(CancellationToken ct);
Task<BarrierStatus> CloseAsync(CancellationToken ct);
```

**ILightService** - Управление светофором
```csharp
Task<LightStatus> SwitchAsync(LightColor color, CancellationToken ct);
```

**IMifareReaderService** - Чтение карт Mifare
```csharp
Task<MifareStatus> StartSearchAsync(CancellationToken ct);
event EventHandler<CardRead>? CardRead;
```

**ISlaveWorker** - Низкоуровневое взаимодействие с оборудованием
```csharp
Task SendCommandAsync(string command, CancellationToken ct);
event EventHandler? VehicleApproached;
event EventHandler? VehiclePassed;
event EventHandler? VehicleReversed;
```

### 4. Entry Coordinator

Оркестрирует весь процесс въезда:
- Обрабатывает доменные события
- Координирует переходы состояний
- Управляет таймаутами
- Проверяет доступ и задолженность

### 5. Timeout Manager

Управление таймаутами операций:
- Таймаут чтения карты (30 сек)
- Таймаут проезда (2 мин)
- Автоматическая отмена при завершении операции

## Пример работы системы

1. **Автомобиль подъезжает** → `VehicleApproached`
   - Светофор → Красный
   - Начинается поиск карты
   - Устанавливается таймаут 30 сек

2. **Карта прочитана** → `CardRead`
   - Отменяется таймаут
   - Проверяются права доступа
   - Проверяется задолженность

3. **Доступ разрешен**
   - Открывается шлагбаум
   - Устанавливается таймаут проезда 2 мин

4. **Автомобиль проехал** → `VehiclePassed` или `VehicleReversed`
   - Закрывается шлагбаум
   - Светофор → Зеленый
   - Возврат в состояние `Idle`

## Технологии

- **.NET 8**
- **Mediator** (с Source Generators)
- **NUnit** - Фреймворк тестирования
- **Moq** - Библиотека моков
- **FluentAssertions** - Fluent API для assertions
- **Entity Framework Core** (опционально для БД)

## Тестирование

### Unit Tests

Изолированное тестирование каждого компонента:
- `EntryStateMachineTests` - Тесты FSM
- `AccessCheckHandlerTests` - Тесты проверки доступа
- `BarrierCommandHandlerTests` - Тесты команд шлагбаума

### Integration Tests

Тестирование полного потока:
```csharp
[Test]
public async Task FullEntryFlow_ValidClient_ShouldOpenAndCloseBarrier()
{
    // Arrange: подготовка данных и моков
    // Act: симуляция событий (VehicleApproached → CardRead → VehiclePassed)
    // Assert: проверка что барьер открылся и закрылся
}
```

### Test Builders

Fluent API для создания тестовых данных:
```csharp
var client = ClientBuilder.Create()
    .WithNumber("12345")
    .WithActiveContract()
    .NotBlocked()
    .WithDebt(100.50m)
    .Build();
```

## Dependency Injection

Регистрация сервисов в `Program.cs`:

```csharp
// Mediator
builder.Services.AddMediator();

// State Machine
builder.Services.AddSingleton<IEntryStateMachine, EntryStateMachine>();
builder.Services.AddSingleton<ITimeoutManager, TimeoutManager>();
builder.Services.AddSingleton<EntryCoordinator>();

// Hardware Services
builder.Services.AddSingleton<IBarrierService, BarrierService>();
builder.Services.AddSingleton<ILightService, LightService>();
builder.Services.AddSingleton<IMifareReaderService, MifareReaderService>();

// Workers
builder.Services.AddSingleton<ISlaveWorker, SlaveWorker>();
builder.Services.AddHostedService<VehicleDetectorWorker>();
```

## Обработка ошибок

### Health Status

Оборудование генерирует события `EquipmentHealthChanged`:
- `Healthy` - Нормальная работа
- `Degraded` - Мягкий отказ (работает с ограничениями)
- `Critical` - Критический отказ (переход в состояние `Error`)

### Таймауты

Автоматическая обработка таймаутов:
- При истечении таймаута чтения карты → возврат в `Idle`
- При истечении таймаута проезда → закрытие шлагбаума и возврат в `Idle`

## Преимущества архитектуры

1. **Слабая связанность** - Компоненты изолированы через Mediator
2. **Тестируемость** - Каждый компонент тестируется независимо
3. **Расширяемость** - Легко добавить новые состояния или команды
4. **Прозрачность** - FSM делает логику переходов явной
5. **Надежность** - Встроенная обработка таймаутов и ошибок

## Следующие шаги

- [ ] Реализация SlaveWorker для взаимодействия с железом
- [ ] Добавление персистентности (Entity Framework Core)
- [ ] Логирование событий в БД
- [ ] Добавление веб-интерфейса мониторинга
- [ ] Метрики и мониторинг состояний
