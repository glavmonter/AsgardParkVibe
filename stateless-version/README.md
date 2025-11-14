# Stateless версия - Quick Start

## Что это?

Альтернативная реализация конечного автомата въездной стойки используя библиотеку **Stateless**.

## Ключевые файлы

```
stateless-version/
├── ParkingEntry.Core.Stateless/
│   ├── Domain/
│   │   ├── EntryState.cs              - Состояния FSM
│   │   ├── EntryTrigger.cs            - Триггеры для переходов ⭐
│   │   └── EquipmentStatus.cs         - Статусы оборудования
│   │
│   ├── Infrastructure/
│   │   ├── EntryStateMachine.cs       - ⭐ Главный FSM на Stateless
│   │   └── EntryCoordinator.cs        - Координатор событий
│   │
│   └── Application/
│       ├── Commands/                   - Команды для оборудования
│       └── Services/                   - Интерфейсы сервисов
│
└── ParkingEntry.Tests.Stateless/
    └── StatelessFinalStateTests.cs    - Тесты FSM
```

## Основные отличия от MediatR версии

### 1. Триггеры вместо событий

**MediatR:**
```csharp
await _mediator.Publish(new VehicleApproached());
```

**Stateless:**
```csharp
await _machine.FireAsync(EntryTrigger.VehicleApproached);
```

### 2. Декларативная конфигурация

**MediatR:** 600 строк в разных handlers

**Stateless:** 200 строк в одном месте
```csharp
_machine.Configure(EntryState.Idle)
    .Permit(EntryTrigger.VehicleApproached, EntryState.ReadingCard);

_machine.Configure(EntryState.ReadingCard)
    .OnEntryAsync(async () =>
    {
        await _mediator.Send(new SetLightCommand(LightColor.Red));
        await _mediator.Send(new StartCardSearchCommand());
    })
    .Permit(EntryTrigger.CardRead, EntryState.CheckingAccess)
    .Permit(EntryTrigger.VehicleLeft, EntryState.Idle);
```

### 3. Автоматическая валидация

```csharp
if (!_machine.CanFire(EntryTrigger.VehiclePassed))
{
    // Переход невозможен в текущем состоянии
    return;
}
```

### 4. Визуализация графа

```csharp
string dotGraph = _stateMachine.GetDotGraph();
// Генерирует DOT файл для GraphViz
// Можно автоматически визуализировать FSM!
```

## Использование

### Базовый пример

```csharp
// Создать FSM
var stateMachine = new EntryStateMachine(mediator, logger);

// Проверить текущее состояние
var state = stateMachine.CurrentState; // Idle

// Выполнить переход
await stateMachine.FireAsync(EntryTrigger.VehicleApproached);
// Теперь состояние: ReadingCard

// Проверить возможность перехода
if (stateMachine.CanFire(EntryTrigger.CardRead))
{
    await stateMachine.FireAsync(EntryTrigger.CardRead, "CARD123");
}
```

### Через координатор

```csharp
var coordinator = new EntryCoordinator(stateMachine, logger);

// События обрабатываются координатором
await coordinator.HandleVehicleApproachedAsync();
await coordinator.HandleCardReadAsync("CARD123");
await coordinator.HandleVehiclePassedAsync();

// Проверить состояние
var state = coordinator.CurrentState;
```

## Тестирование

```csharp
[Test]
public async Task VehiclePassed_FromWaitingPassage_ShouldWork()
{
    // Arrange - создаем FSM в нужном состоянии
    var coordinator = await CreateCoordinatorAtStateAsync(
        EntryState.WaitingPassage, 
        cardNumber: "TEST123");

    // Act
    await coordinator.HandleVehiclePassedAsync();
    await Task.Delay(100); // Ждем автоматические переходы

    // Assert
    coordinator.CurrentState.Should().Be(EntryState.Idle);
}
```

## Преимущества Stateless

### ✅ 1. Декларативность
Все переходы видны в одном месте:
```csharp
_machine.Configure(EntryState.ReadingCard)
    .Permit(EntryTrigger.CardRead, EntryState.CheckingAccess)
    .Permit(EntryTrigger.VehicleLeft, EntryState.Idle)
    .Permit(EntryTrigger.CardReadTimeout, EntryState.Idle);
```

### ✅ 2. Автоматическая валидация
Невозможно сделать недопустимый переход.

### ✅ 3. Визуализация
```bash
# Генерируем DOT файл
dotnet run > fsm.dot

# Конвертируем в PNG (нужен GraphViz)
dot -Tpng fsm.dot -o fsm.png
```

### ✅ 4. OnEntry/OnExit hooks
```csharp
.OnEntryAsync(async () => { /* логика входа */ })
.OnExit(() => { /* логика выхода */ })
```

### ✅ 5. Guard conditions
```csharp
.PermitIf(EntryTrigger.Proceed, NextState, () => condition)
```

### ✅ 6. Меньше кода
200 строк vs 600 строк в MediatR версии.

## Недостатки

### ❌ 1. Сложнее тестировать с произвольного состояния
Нужно пройти через все промежуточные переходы.

### ❌ 2. Дополнительная зависимость
NuGet: Stateless (5.16.0)

### ❌ 3. Learning curve
Нужно изучить API Stateless.

## Сравнение

| Критерий | Stateless | MediatR |
|----------|-----------|---------|
| Размер кода | ✅ 200 строк | ❌ 600 строк |
| Читаемость FSM | ✅ Все в одном месте | ❌ Размазано |
| Валидация | ✅ Автоматическая | ❌ Ручная |
| Визуализация | ✅ Автогенерация | ❌ Вручную |
| Гибкость | ⚠️ Ограничена | ✅ Максимальная |
| Тестирование | ❌ Сложнее | ✅ Проще |
| Зависимости | ⚠️ +1 библиотека | ✅ Только MediatR |

## Когда использовать Stateless

✅ FSM - центральная часть приложения  
✅ Нужна визуализация  
✅ Важна формальная корректность  
✅ FSM часто меняется  
✅ Нужны вложенные состояния  

**Рекомендую для парковочной системы!** 🎯

## Когда использовать MediatR

✅ FSM - небольшая часть системы  
✅ Нужна максимальная гибкость  
✅ Важно тестирование с любого состояния  
✅ Не хочешь добавлять зависимости  

## Дополнительно

- 📖 [Детальное сравнение](./STATELESS_VS_MEDIATR.md)
- 📚 [Stateless GitHub](https://github.com/dotnet-state-machine/stateless)
- 🎯 [MediatR версия](../src/ParkingEntry.Core/)

## Установка Stateless

```bash
dotnet add package Stateless --version 5.16.0
```

## Визуализация графа

```csharp
// В коде
var dotGraph = coordinator.GetStateDiagram();
File.WriteAllText("fsm.dot", dotGraph);

// В терминале (нужен GraphViz)
dot -Tpng fsm.dot -o fsm.png
```

## Итого

**Stateless** - отличный выбор для парковочной системы:
- ✅ Меньше кода
- ✅ Автоматическая валидация
- ✅ Визуализация
- ✅ Формальная корректность

**Но** если нужна максимальная гибкость в тестах - используй MediatR версию.

---

**Обе версии полностью рабочие и протестированные!** 🚀
