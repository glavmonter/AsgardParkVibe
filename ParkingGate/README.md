# Parking Gate Control System

Система управления въездной стойкой парковки на базе Mediator паттерна с использованием MediatR.

## Архитектура

Решение построено на Clean Architecture с разделением на слои:

### Структура проекта

```
ParkingGate/
├── ParkingGate.Domain/          # Доменные модели, события, команды
│   ├── Models/                  # BarrierStatus, LightStatus, GateState
│   ├── Events/                  # VehicleApproachedEvent, CardReadEvent
│   ├── Commands/                # OpenBarrierCommand, SetLightColorCommand
│   └── Queries/                 # GetGateStateQuery
├── ParkingGate.Infrastructure/  # Реализация аппаратных интерфейсов
│   ├── Hardware/                # ISlave, IMifareReader, Mock реализации
│   └── Services/                # GateStateService
├── ParkingGate.Application/     # Бизнес-логика и обработчики
│   └── Handlers/
│       ├── Commands/            # Обработчики команд
│       ├── Events/              # Обработчики событий
│       └── Queries/             # Обработчики запросов
├── ParkingGate.Api/             # ASP.NET Core Web API
│   └── Controllers/             # StatusController, SimulationController
└── ParkingGate.Tests/           # NUnit тесты
    └── Handlers/                # Тесты обработчиков
```

## Ключевые компоненты

### События автомобиля

- `VehicleApproachedEvent` - автомобиль подъехал к шлагбауму
- `VehicleDepartedEvent` - автомобиль уехал от шлагбаума
- `VehiclePassedThroughEvent` - автомобиль проехал через шлагбаум
- `VehicleBackedOutEvent` - автомобиль заехал за шлагбаум, но уехал назад

### События Mifare ридера

- `CardReadEvent` - карта прочитана
- `CardWrittenEvent` - карта записана
- `HealthStatusChangedEvent` - изменился статус здоровья оборудования

### Команды управления

**Шлагбаум:**
- `OpenBarrierCommand` - открыть шлагбаум
- `CloseBarrierCommand` - закрыть шлагбаум

**Светофор:**
- `SetLightColorCommand` - переключить цвет (Red, Green, Both, None)

**Mifare ридер:**
- `StartSearchingCardCommand` - начать поиск карты
- `StopSearchingCardCommand` - остановить поиск карты
- `WriteCardCommand` - записать карту
- `StopWritingCardCommand` - остановить запись карты

**Бизнес-логика:**
- `CheckAccessRightsCommand` - проверить права доступа
- `CalculateDebtCommand` - посчитать задолженность

### Сервисы

- `IGateStateService` - централизованное хранение состояния ворот (singleton, thread-safe)
- `ISlave` - интерфейс для управления шлагбаумом и светофором
- `IMifareReader` - интерфейс для работы с Mifare картами

## Алгоритм работы

### Успешный въезд

1. **Автомобиль подъезжает** (`VehicleApproachedEvent`)
   - Светофор переключается на красный
   - Начинается поиск карты

2. **Карта прочитана** (`CardReadEvent`)
   - Останавливается поиск карты
   - Проверяются права доступа
   - Если доступ разрешен → открывается шлагбаум
   - Если доступ запрещен → шлагбаум остается закрытым

3. **Автомобиль проехал** (`VehiclePassedThroughEvent`)
   - Закрывается шлагбаум
   - Светофор переключается на зеленый
   - Система возвращается в состояние ожидания

### Отмена въезда

Если автомобиль уехал (`VehicleDepartedEvent` или `VehicleBackedOutEvent`):
- Закрывается шлагбаум (если был открыт)
- Светофор переключается на зеленый
- Система возвращается в состояние ожидания

## API Endpoints

### Status Controller

```
GET /api/status          - Получить текущее состояние ворот
GET /api/status/health   - Health check
```

### Simulation Controller

```
POST /api/simulation/vehicle/approached      - Имитация подъезда автомобиля
POST /api/simulation/vehicle/departed        - Имитация отъезда автомобиля
POST /api/simulation/vehicle/passed-through  - Имитация проезда автомобиля
POST /api/simulation/vehicle/backed-out      - Имитация движения назад
POST /api/simulation/card/read              - Имитация чтения карты
  Body: { "cardNumber": "1234" }
```

## Запуск

### Требования

- .NET 8.0 SDK
- Visual Studio 2022 / JetBrains Rider / VS Code

### Запуск API

```bash
cd ParkingGate.Api
dotnet run
```

API будет доступен по адресу: `https://localhost:7001` (или `http://localhost:5001`)

Swagger UI: `https://localhost:7001/swagger`

### Запуск тестов

```bash
cd ParkingGate.Tests
dotnet test
```

## Примеры использования

### 1. Проверка состояния системы

```bash
curl https://localhost:7001/api/status
```

Ответ:
```json
{
  "mode": "Entry",
  "barrierStatus": "Closed",
  "lightStatus": "Green",
  "mifareStatus": "Idle",
  "systemHealth": "Healthy",
  "lastError": null,
  "lastUpdate": "2025-11-13T10:30:00Z",
  "currentStateName": "WaitingForVehicle"
}
```

### 2. Имитация полного сценария въезда

```bash
# Шаг 1: Автомобиль подъезжает
curl -X POST https://localhost:7001/api/simulation/vehicle/approached

# Шаг 2: Карта читается (автоматически через 2-5 секунд)
# ИЛИ можно имитировать вручную:
curl -X POST https://localhost:7001/api/simulation/card/read \
  -H "Content-Type: application/json" \
  -d '{"cardNumber": "1234"}'

# Шаг 3: Автомобиль проехал
curl -X POST https://localhost:7001/api/simulation/vehicle/passed-through

# Проверка состояния
curl https://localhost:7001/api/status
```

### 3. Сценарий отказа в доступе

```bash
# Автомобиль подъезжает
curl -X POST https://localhost:7001/api/simulation/vehicle/approached

# Карта с отказом (номер не начинается с "1")
curl -X POST https://localhost:7001/api/simulation/card/read \
  -H "Content-Type: application/json" \
  -d '{"cardNumber": "9999"}'

# Автомобиль уезжает
curl -X POST https://localhost:7001/api/simulation/vehicle/departed
```

## Тестирование

Проект включает три набора тестов:

### 1. TrafficLightBehaviorTests

Проверяет поведение светофора:
- Красный при подъезде автомобиля
- Зеленый при уезде, проезде или движении назад

### 2. CardSearchBehaviorTests

Проверяет включение поиска карты при подъезде автомобиля.

### 3. FullEntryScenarioTests

Интеграционные тесты полных сценариев:
- Успешный въезд с доступом
- Отказ в доступе
- Последовательность переходов состояний

## Особенности реализации

### Thread-Safe State Management

`GateStateService` использует `lock` для обеспечения потокобезопасности при обновлении состояния из разных обработчиков.

### Singleton FSM

Конечный автомат работает как singleton - все события обрабатываются последовательно через MediatR.

### Mock Hardware

Для разработки и тестирования используются mock реализации:
- `MockSlave` - имитирует работу с шлагбаумом и светофором
- `MockMifareReader` - имитирует чтение карт (автоматически через 2-5 секунд)

### Режимы работы

Система поддерживает два режима (через `GateMode` enum):
- `Entry` - въездная стойка (реализован)
- `Exit` - выездная стойка (для будущей реализации)

## Расширение системы

### Добавление новых состояний

1. Создайте новое событие в `Domain/Events`
2. Создайте обработчик в `Application/Handlers/Events`
3. Обновите `GateStateService` при необходимости
4. Добавьте тесты

### Подключение реального оборудования

Замените mock реализации на реальные:

```csharp
// В Program.cs замените:
builder.Services.AddSingleton<ISlave, RealSlaveImplementation>();
builder.Services.AddSingleton<IMifareReader, RealMifareReaderImplementation>();
```

### Добавление таймаутов

Для реализации таймаутов можно использовать `CancellationTokenSource`:

```csharp
var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
try 
{
    await _mediator.Send(command, cts.Token);
}
catch (OperationCanceledException)
{
    // Обработка таймаута
}
```

## Зависимости

- **MediatR** - паттерн Mediator
- **FluentAssertions** - читаемые assertions в тестах
- **NUnit** - тестовый фреймворк
- **Moq** - мок-объекты для тестов
- **Swashbuckle** - Swagger/OpenAPI документация

## Лицензия

MIT
