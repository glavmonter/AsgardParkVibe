# Quick Reference Guide

## Команды быстрого доступа

### Разработка

```bash
# Запуск API
dotnet run --project src/ParkingEntry.Api

# Запуск тестов
dotnet test

# Запуск конкретных тестов
dotnet test --filter "LightBehaviorTests"
dotnet test --filter "FinalStateTests"

# Watch mode (авто-перезапуск при изменениях)
dotnet watch --project src/ParkingEntry.Api
```

### API Endpoints (быстрая шпаргалка)

```bash
BASE_URL="https://localhost:5001/api/entry"

# Статус
curl -k GET $BASE_URL/status

# События
curl -k POST $BASE_URL/events/vehicle-approached
curl -k POST $BASE_URL/events/vehicle-left
curl -k POST $BASE_URL/events/vehicle-passed
curl -k POST $BASE_URL/events/vehicle-reversed
curl -k POST $BASE_URL/events/card-read -H "Content-Type: application/json" -d '{"cardNumber":"TEST"}'

# Сброс
curl -k POST $BASE_URL/reset
```

## Состояния и переходы (краткая версия)

```
Idle → VehicleApproached → ReadingCard
ReadingCard → CardRead → CheckingAccess
CheckingAccess → (Access Allowed) → WaitingPassage
CheckingAccess → (Access Denied) → AccessDenied
WaitingPassage → VehiclePassed → Idle
WaitingPassage → VehicleReversed → Idle
ReadingCard → VehicleLeft → Idle
AccessDenied → VehicleLeft → Idle
```

## Типичные сценарии тестирования

### Сценарий 1: Успешный въезд
```bash
curl -k POST $BASE_URL/reset
curl -k POST $BASE_URL/events/vehicle-approached
curl -k POST $BASE_URL/events/card-read -H "Content-Type: application/json" -d '{"cardNumber":"CARD123"}'
curl -k POST $BASE_URL/events/vehicle-passed
curl -k GET $BASE_URL/status  # Должен быть Idle
```

### Сценарий 2: Отказ в доступе
```bash
curl -k POST $BASE_URL/reset
curl -k POST $BASE_URL/events/vehicle-approached
curl -k POST $BASE_URL/events/card-read -H "Content-Type: application/json" -d '{"cardNumber":"DENIED"}'
curl -k GET $BASE_URL/status  # Должен быть AccessDenied
curl -k POST $BASE_URL/events/vehicle-left
```

### Сценарий 3: Автомобиль уехал до чтения карты
```bash
curl -k POST $BASE_URL/reset
curl -k POST $BASE_URL/events/vehicle-approached
curl -k POST $BASE_URL/events/vehicle-left
curl -k GET $BASE_URL/status  # Должен быть Idle
```

## Структура проекта (краткая)

```
ParkingEntry/
├── src/
│   ├── ParkingEntry.Core/      # Бизнес-логика
│   │   ├── Domain/             # Модели и события
│   │   ├── Application/        # Handlers и команды
│   │   └── Infrastructure/     # Реализации
│   └── ParkingEntry.Api/       # ASP.NET API
├── tests/
│   └── ParkingEntry.Tests/     # Unit-тесты
├── README.md                    # Основная документация
├── ARCHITECTURE.md              # Детальная архитектура
└── DEPLOYMENT.md                # Развертывание
```

## Ключевые файлы для изменения

### Добавить новое событие:
1. `Core/Domain/Events/DomainEvents.cs` - определить событие
2. `Core/Application/Handlers/` - создать handler
3. `Api/Controllers/EntryController.cs` - добавить endpoint (опционально)

### Добавить новый сервис оборудования:
1. `Core/Application/Services/IEquipmentServices.cs` - определить интерфейс
2. `Core/Application/Commands/Commands.cs` - добавить команды
3. `Core/Application/Handlers/EquipmentCommandHandlers.cs` - создать handlers
4. `Api/Services/MockServices.cs` - создать mock (для тестов)
5. `Api/Program.cs` - зарегистрировать

### Изменить логику переходов состояний:
1. `Core/Application/Handlers/` - изменить соответствующий handler
2. `Tests/` - обновить тесты

## Частые ошибки и решения

### Порт занят
```bash
# Найти процесс
sudo netstat -tulpn | grep 5000
# Или
sudo lsof -i :5000

# Убить процесс
kill -9 <PID>
```

### Тесты не проходят после изменений
```bash
dotnet clean
dotnet restore
dotnet build
dotnet test
```

### Mock не работает в тестах
Проверить что:
1. Setup вызывается в [SetUp] или конструкторе
2. Mock.Verify использует правильные параметры (It.IsAny<>)
3. StateContext в правильном состоянии

## Полезные команды NuGet

```bash
# Обновить все пакеты
dotnet list package --outdated
dotnet add package <PackageName> --version <Version>

# Добавить новый пакет
dotnet add src/ParkingEntry.Core/ParkingEntry.Core.csproj package <PackageName>
```

## Git (если используется)

```bash
# Игнорировать
cat >> .gitignore << EOF
bin/
obj/
*.user
.vs/
.idea/
*.suo
publish/
logs/
EOF

# Первый коммит
git init
git add .
git commit -m "Initial commit: Parking Entry System with Mediator"
```

## Docker (quick reference)

```bash
# Сборка
docker build -t parking-entry:latest .

# Запуск
docker run -d -p 5000:5000 --name parking-entry parking-entry:latest

# Логи
docker logs -f parking-entry

# Остановка и удаление
docker stop parking-entry && docker rm parking-entry
```

## Мониторинг в production

```bash
# CPU и Memory
top -p $(pgrep -f ParkingEntry)

# Логи в реальном времени
sudo journalctl -u parking-entry -f

# Проверка health
curl http://localhost:5000/health

# Статус службы
sudo systemctl status parking-entry
```

## Полезные VS Code расширения

- C# Dev Kit
- REST Client (для тестирования API без Postman)
- Docker
- GitLens

## Полезные Rider/VS настройки

- Enable Hot Reload
- Enable Solution-Wide Analysis
- StyleCop включен - следить за предупреждениями

## Производительность

### Для терминалов ARM64:
- Используйте --self-contained для избежания зависимости от runtime
- Ограничьте MaxConcurrentConnections в Kestrel
- Используйте SQLite вместо MSSQL если возможно

### Для тестов:
- Используйте [Parallelizable] где возможно
- Mock тяжелые операции
- Не делайте реальные HTTP запросы в unit-тестах

## Контакты

- Документация проекта: README.md
- Архитектура: ARCHITECTURE.md
- Деплой: DEPLOYMENT.md
- Примеры API: API_TESTING_GUIDE.md

## Версии зависимостей

- .NET: 8.0
- MediatR: 12.4.1
- NUnit: 4.2.2
- FluentAssertions: 6.12.1
- Moq: 4.20.72

## Checklist перед деплоем

- [ ] Все тесты проходят
- [ ] Нет StyleCop warnings
- [ ] Заменены Mock сервисы на реальные
- [ ] Настроен appsettings.Production.json
- [ ] Настроено логирование
- [ ] Добавлены health checks
- [ ] Настроена безопасность (HTTPS, auth если нужно)
- [ ] Проверена производительность
- [ ] Настроен systemd service / Docker
- [ ] Настроен мониторинг
