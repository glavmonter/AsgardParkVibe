# Диаграмма конечного автомата

```mermaid
stateDiagram-v2
    [*] --> Created: CreateOrder
    
    Created --> Validated: ValidationSucceeded
    Created --> Failed: ValidationFailed
    
    Validated --> Processing: ProcessingStarted
    
    Processing --> Completed: ProcessingCompleted
    Processing --> Failed: ProcessingFailed
    
    Failed --> Created: Reset
    Completed --> Created: Reset
    
    Completed --> [*]
    Failed --> [*]
    
    note right of Created
        OrderCreationService
        создает заказы
    end note
    
    note right of Validated
        OrderValidationService
        валидирует заказы
    end note
    
    note right of Processing
        OrderProcessingService
        обрабатывает заказы
    end note
```

## Описание переходов

| Текущее состояние | Событие | Новое состояние | Описание |
|-------------------|---------|-----------------|----------|
| Created | ValidationSucceeded | Validated | Заказ прошел валидацию |
| Created | ValidationFailed | Failed | Заказ не прошел валидацию |
| Validated | ProcessingStarted | Processing | Начата обработка заказа |
| Processing | ProcessingCompleted | Completed | Обработка успешно завершена |
| Processing | ProcessingFailed | Failed | Ошибка при обработке |
| Failed | Reset | Created | Сброс провального заказа |
| Completed | Reset | Created | Сброс завершенного заказа |

## Схема взаимодействия компонентов

```mermaid
graph TB
    subgraph "Hosted Services"
        HS1[OrderCreationService]
        HS2[OrderValidationService]
        HS3[OrderProcessingService]
        HS4[StatisticsService]
    end
    
    subgraph "MediatR"
        CMD1[CreateOrderCommand]
        CMD2[ValidateOrderCommand]
        CMD3[ProcessOrderCommand]
        
        NOT1[OrderCreatedNotification]
        NOT2[OrderValidatedNotification]
        NOT3[OrderProcessedNotification]
        
        H1[CreateOrderHandler]
        H2[ValidateOrderHandler]
        H3[ProcessOrderHandler]
    end
    
    subgraph "State Machine - Singleton"
        SM[OrderStateMachine]
    end
    
    HS1 -->|Send| CMD1
    HS2 -->|Send| CMD2
    HS3 -->|Send| CMD3
    HS4 -->|Read| SM
    
    CMD1 -->|Handle| H1
    CMD2 -->|Handle| H2
    CMD3 -->|Handle| H3
    
    H1 -->|Update| SM
    H2 -->|Update| SM
    H3 -->|Update| SM
    
    H1 -->|Publish| NOT1
    H2 -->|Publish| NOT2
    H3 -->|Publish| NOT3
    
    style SM fill:#f9f,stroke:#333,stroke-width:4px
    style HS1 fill:#bbf,stroke:#333,stroke-width:2px
    style HS2 fill:#bbf,stroke:#333,stroke-width:2px
    style HS3 fill:#bbf,stroke:#333,stroke-width:2px
    style HS4 fill:#bfb,stroke:#333,stroke-width:2px
```

## Временная диаграмма

```mermaid
sequenceDiagram
    participant OCS as OrderCreationService
    participant M as MediatR
    participant H1 as CreateOrderHandler
    participant SM as StateMachine
    participant OVS as OrderValidationService
    participant H2 as ValidateOrderHandler
    participant OPS as OrderProcessingService
    participant H3 as ProcessOrderHandler
    
    OCS->>M: Send CreateOrderCommand
    M->>H1: Handle CreateOrderCommand
    H1->>SM: CreateOrderAsync(orderId)
    SM-->>H1: true
    H1->>M: Publish OrderCreatedNotification
    H1-->>OCS: true
    
    Note over SM: State: Created
    
    OVS->>SM: GetOrdersInState(Created)
    SM-->>OVS: [orderId]
    OVS->>M: Send ValidateOrderCommand
    M->>H2: Handle ValidateOrderCommand
    H2->>SM: TriggerEventAsync(ValidationSucceeded)
    SM-->>H2: true
    H2->>M: Publish OrderValidatedNotification
    H2-->>OVS: true
    
    Note over SM: State: Validated
    
    OPS->>SM: GetOrdersInState(Validated)
    SM-->>OPS: [orderId]
    OPS->>M: Send ProcessOrderCommand
    M->>H3: Handle ProcessOrderCommand
    H3->>SM: TriggerEventAsync(ProcessingStarted)
    SM-->>H3: true
    
    Note over SM: State: Processing
    
    H3->>SM: TriggerEventAsync(ProcessingCompleted)
    SM-->>H3: true
    H3->>M: Publish OrderProcessedNotification
    H3-->>OPS: true
    
    Note over SM: State: Completed
```
