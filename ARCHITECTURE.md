# Детальная архитектура системы

## Общая архитектура

```
┌───────────────────────────────────────────────────────────────────┐
│                         Client Layer                               │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐         │
│  │ Browser  │  │  Postman │  │   cURL   │  │  Python  │         │
│  │ Swagger  │  │          │  │          │  │  Script  │         │
│  └─────┬────┘  └────┬─────┘  └────┬─────┘  └────┬─────┘         │
│        │            │             │             │                 │
│        └────────────┴─────────────┴─────────────┘                 │
│                            │ HTTPS                                │
└────────────────────────────┼──────────────────────────────────────┘
                             ▼
┌───────────────────────────────────────────────────────────────────┐
│                      ASP.NET Core API                              │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │                   EntryController                            │ │
│  │  • GET  /api/entry/status                                   │ │
│  │  • POST /api/entry/events/vehicle-approached                │ │
│  │  • POST /api/entry/events/card-read                         │ │
│  │  • POST /api/entry/events/vehicle-passed                    │ │
│  │  • POST /api/entry/events/vehicle-reversed                  │ │
│  │  • POST /api/entry/events/vehicle-left                      │ │
│  │  • POST /api/entry/reset                                    │ │
│  └─────────────────────────────────────────────────────────────┘ │
└────────────────────────────┬──────────────────────────────────────┘
                             ▼
┌───────────────────────────────────────────────────────────────────┐
│                        MediatR Bus                                 │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │              Events (INotification)                          │ │
│  │  • VehicleApproached    • CardRead                          │ │
│  │  • VehicleLeft          • CardWritten                       │ │
│  │  • VehiclePassed        • OperationTimeout                  │ │
│  │  • VehicleReversed      • EquipmentHealthChanged           │ │
│  └─────────────────────────────────────────────────────────────┘ │
│  ┌─────────────────────────────────────────────────────────────┐ │
│  │              Commands (IRequest<T>)                          │ │
│  │  • OpenBarrierCommand   • StartCardSearchCommand            │ │
│  │  • CloseBarrierCommand  • StopCardSearchCommand             │ │
│  │  • SetLightCommand      • CheckAccessCommand                │ │
│  │  • WriteCardCommand     • CheckDebtCommand                  │ │
│  └─────────────────────────────────────────────────────────────┘ │
└────────────────────────────┬──────────────────────────────────────┘
                             ▼
┌───────────────────────────────────────────────────────────────────┐
│                    Application Layer                               │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────────┐│
│  │                    Event Handlers                             ││
│  │  ┌─────────────────────┐  ┌─────────────────────┐           ││
│  │  │ VehicleApproached   │  │    CardRead         │           ││
│  │  │     Handler         │  │    Handler          │           ││
│  │  │                     │  │                     │           ││
│  │  │ • Set Red Light     │  │ • Stop Search       │           ││
│  │  │ • Start Card Search │  │ • Check Access      │           ││
│  │  │ • → ReadingCard     │  │ • Open Barrier      │           ││
│  │  └─────────────────────┘  └─────────────────────┘           ││
│  │                                                               ││
│  │  ┌─────────────────────┐  ┌─────────────────────┐           ││
│  │  │  VehiclePassed      │  │  VehicleReversed    │           ││
│  │  │     Handler         │  │     Handler         │           ││
│  │  │                     │  │                     │           ││
│  │  │ • Close Barrier     │  │ • Close Barrier     │           ││
│  │  │ • Set Green Light   │  │ • Set Green Light   │           ││
│  │  │ • → Idle            │  │ • → Idle            │           ││
│  │  └─────────────────────┘  └─────────────────────┘           ││
│  └──────────────────────────────────────────────────────────────┘│
│                                                                    │
│  ┌──────────────────────────────────────────────────────────────┐│
│  │                   Command Handlers                            ││
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐       ││
│  │  │   Barrier    │  │    Light     │  │   Mifare     │       ││
│  │  │   Command    │  │   Command    │  │   Command    │       ││
│  │  │   Handler    │  │   Handler    │  │   Handler    │       ││
│  │  └──────────────┘  └──────────────┘  └──────────────┘       ││
│  │  ┌──────────────┐  ┌──────────────┐                         ││
│  │  │   Access     │  │     Debt     │                         ││
│  │  │   Check      │  │    Check     │                         ││
│  │  │   Handler    │  │   Handler    │                         ││
│  │  └──────────────┘  └──────────────┘                         ││
│  └──────────────────────────────────────────────────────────────┘│
│                                                                    │
│  ┌──────────────────────────────────────────────────────────────┐│
│  │               StateContext (Singleton)                        ││
│  │  ┌────────────────────────────────────────────┐              ││
│  │  │  • CurrentState: EntryState                │              ││
│  │  │  • CurrentCardNumber: string?              │              ││
│  │  │  • SetState(state)                         │              ││
│  │  │  • SetCardNumber(card)                     │              ││
│  │  │  • Reset()                                 │              ││
│  │  │                                            │              ││
│  │  │  Thread-safe with lock                    │              ││
│  │  └────────────────────────────────────────────┘              ││
│  └──────────────────────────────────────────────────────────────┘│
└────────────────────────────┬──────────────────────────────────────┘
                             ▼
┌───────────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                            │
│                                                                    │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐              │
│  │  IBarrier   │  │   ILight    │  │  IMifare    │              │
│  │  Service    │  │   Service   │  │  Service    │              │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘              │
│         │                │                │                      │
│         ▼                ▼                ▼                      │
│  ┌──────────────────────────────────────────────────┐           │
│  │           Hardware Abstraction Layer              │           │
│  │  (ISlave Worker для Barrier/Light)               │           │
│  │  (IMifareReader Worker для Mifare)               │           │
│  └──────────────────────────────────────────────────┘           │
│         │                │                │                      │
│         ▼                ▼                ▼                      │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐             │
│  │  Physical   │  │  Traffic    │  │   Card      │             │
│  │  Barrier    │  │   Light     │  │  Reader     │             │
│  └─────────────┘  └─────────────┘  └─────────────┘             │
│                                                                    │
│  ┌─────────────────────────────────────────────────┐             │
│  │            Business Services                     │             │
│  │  ┌─────────────────┐  ┌──────────────────┐     │             │
│  │  │ IAccessCheck    │  │  IDebtCheck      │     │             │
│  │  │   Service       │  │   Service        │     │             │
│  │  └────────┬────────┘  └─────────┬────────┘     │             │
│  │           │                     │               │             │
│  │           └──────────┬──────────┘               │             │
│  │                      ▼                          │             │
│  │            ┌─────────────────┐                  │             │
│  │            │    Database     │                  │             │
│  │            │   (MSSQL/SQLite)│                  │             │
│  │            └─────────────────┘                  │             │
│  └─────────────────────────────────────────────────┘             │
│                                                                    │
│  ┌──────────────────────────────────────────────────────────────┐│
│  │              TimeoutManager                                   ││
│  │  • StartTimerAsync(operation, timeout, onTimeout)            ││
│  │  • CancelTimer(timerId)                                      ││
│  │  • IsTimerActive(timerId)                                    ││
│  └──────────────────────────────────────────────────────────────┘│
└───────────────────────────────────────────────────────────────────┘
```

## Поток обработки события

### Пример: VehicleApproached

```
┌──────────────────┐
│   HTTP Request   │
│  POST /events/   │
│vehicle-approached│
└────────┬─────────┘
         │
         ▼
┌─────────────────────────────────────┐
│      EntryController                │
│  await _mediator.Publish(           │
│    new VehicleApproached());        │
└────────┬────────────────────────────┘
         │
         ▼
┌─────────────────────────────────────┐
│      MediatR Pipeline               │
│  Находит все INotificationHandlers  │
└────────┬────────────────────────────┘
         │
         ▼
┌────────────────────────────────────────┐
│  VehicleApproachedHandler              │
│  1. Check: CurrentState == Idle?       │
│  2. Send(SetLightCommand(Red))         │
│  3. Send(StartCardSearchCommand())     │
│  4. SetState(ReadingCard)              │
└────────┬───────────────────────────────┘
         │
    ┌────┴────┐
    ▼         ▼
┌─────────┐ ┌─────────────────┐
│  Light  │ │  Mifare         │
│ Handler │ │  Handler        │
└────┬────┘ └────┬────────────┘
     │           │
     ▼           ▼
┌──────────┐ ┌──────────────┐
│ ILight   │ │ IMifare      │
│ Service  │ │ Service      │
└──────────┘ └──────────────┘
```

## State Transition Details

```
State: Idle
Event: VehicleApproached
Actions:
  1. LightService.SetColor(Red)
  2. MifareService.StartSearch()
  3. StateContext.SetState(ReadingCard)
Next State: ReadingCard

─────────────────────────────────────

State: ReadingCard
Event: CardRead(cardNumber)
Actions:
  1. StateContext.SetCardNumber(cardNumber)
  2. MifareService.StopSearch()
  3. StateContext.SetState(CheckingAccess)
  4. AccessCheckService.CheckAccess(cardNumber)
     ├─ If Allowed:
     │  ├─ StateContext.SetState(OpeningBarrier)
     │  ├─ BarrierService.Open()
     │  └─ StateContext.SetState(WaitingPassage)
     └─ If Denied:
        └─ StateContext.SetState(AccessDenied)

─────────────────────────────────────

State: WaitingPassage
Event: VehiclePassed OR VehicleReversed
Actions:
  1. StateContext.SetState(ClosingBarrier)
  2. BarrierService.Close()
  3. LightService.SetColor(Green)
  4. StateContext.Reset()
Next State: Idle

─────────────────────────────────────

State: ReadingCard OR AccessDenied
Event: VehicleLeft
Actions:
  1. If ReadingCard: MifareService.StopSearch()
  2. LightService.SetColor(Green)
  3. StateContext.Reset()
Next State: Idle
```

## Dependency Injection Container

```
ServiceCollection
├─ Singleton
│  ├─ IStateContext → StateContext
│  ├─ ITimeoutManager → TimeoutManager
│  ├─ IBarrierService → MockBarrierService (заменить на реальный)
│  ├─ ILightService → MockLightService
│  ├─ IMifareService → MockMifareService
│  ├─ IAccessCheckService → MockAccessCheckService
│  └─ IDebtCheckService → MockDebtCheckService
│
└─ MediatR (Transient/Scoped)
   ├─ IMediator
   ├─ All INotificationHandlers
   └─ All IRequestHandlers
```

## Testing Architecture

```
Test Base Class: EntryStateMachineTestBase
├─ Setup(initialState, cardNumber?)
│  ├─ Creates all Mocks
│  ├─ Configures default behaviors
│  ├─ Builds ServiceProvider
│  ├─ Resolves IMediator
│  └─ Sets initial StateContext
│
└─ Helper Methods
   ├─ AssertState(expected)
   └─ AssertCardNumber(expected)

Test Class extends Base
└─ [Test] методы
   ├─ Arrange: Setup(specificState)
   ├─ Act: await Mediator.Publish(event)
   └─ Assert: Verify mocks, AssertState()
```

## Преимущества этой архитектуры

### 1. Модульность
Каждый handler - отдельный класс с одной ответственностью

### 2. Тестируемость
```csharp
// Можно начать с ЛЮБОГО состояния!
Setup(EntryState.WaitingPassage);
await Mediator.Publish(new VehiclePassed());
// Без прохождения через Idle → ReadingCard → etc
```

### 3. Расширяемость
Добавление нового события:
1. Создать record Event : DomainEvent
2. Создать class Handler : INotificationHandler<Event>
3. Зарегистрируется автоматически

### 4. Независимость
- API не зависит от реализации сервисов
- Handlers не зависят друг от друга
- Легко заменить оборудование

### 5. Безопасность потоков
StateContext использует lock для thread-safety

## Production Considerations

### Для реальной эксплуатации заменить:

1. **Mock Services** → Реальные реализации с ISlave/IMifareReader
2. **In-Memory State** → Персистентное хранилище (Redis/DB)
3. **Logging** → Структурированное логирование (Serilog)
4. **Error Handling** → Circuit Breaker, Retry Policies
5. **Monitoring** → Health checks, Metrics (Prometheus)
6. **Security** → Authentication, Authorization
7. **Configuration** → appsettings.json, Environment variables

### Масштабирование:

- Для нескольких стоек: отдельный StateContext для каждой
- Использовать SignalR для real-time updates в UI
- Event Sourcing для audit trail
