# Руководство по развертыванию

## Быстрый старт (Development)

### Требования
- .NET 8.0 SDK
- Visual Studio 2022 / Rider / VS Code
- Git (опционально)

### Запуск за 3 шага

```bash
# 1. Клонировать/распаковать проект
cd ParkingEntry

# 2. Восстановить зависимости
dotnet restore

# 3. Запустить API
cd src/ParkingEntry.Api
dotnet run
```

API будет доступен:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger: `https://localhost:5001/swagger`

### Запуск тестов

```bash
# Все тесты
dotnet test

# С выводом деталей
dotnet test --logger "console;verbosity=detailed"

# Конкретный тест
dotnet test --filter "FullyQualifiedName~FinalStateTests"
```

---

## Production Deployment

### Вариант 1: Standalone Executable

#### Сборка

```bash
# Linux ARM64 (для Raspberry Pi / терминалов)
dotnet publish src/ParkingEntry.Api/ParkingEntry.Api.csproj \
  -c Release \
  -r linux-arm64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -o ./publish/linux-arm64

# Linux x64
dotnet publish src/ParkingEntry.Api/ParkingEntry.Api.csproj \
  -c Release \
  -r linux-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -o ./publish/linux-x64

# Windows x64
dotnet publish src/ParkingEntry.Api/ParkingEntry.Api.csproj \
  -c Release \
  -r win-x64 \
  --self-contained true \
  -p:PublishSingleFile=true \
  -o ./publish/win-x64
```

#### Запуск

```bash
# Linux
cd publish/linux-arm64
chmod +x ParkingEntry.Api
./ParkingEntry.Api

# Windows
cd publish\win-x64
ParkingEntry.Api.exe
```

### Вариант 2: Systemd Service (Linux)

#### Создать service файл

```bash
sudo nano /etc/systemd/system/parking-entry.service
```

```ini
[Unit]
Description=Parking Entry Gate Service
After=network.target

[Service]
Type=notify
User=parking
Group=parking
WorkingDirectory=/opt/parking-entry
ExecStart=/opt/parking-entry/ParkingEntry.Api
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=parking-entry
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://0.0.0.0:5000

[Install]
WantedBy=multi-user.target
```

#### Настройка

```bash
# Создать пользователя
sudo useradd -r -s /bin/false parking

# Скопировать файлы
sudo mkdir -p /opt/parking-entry
sudo cp -r publish/linux-arm64/* /opt/parking-entry/
sudo chown -R parking:parking /opt/parking-entry

# Включить и запустить
sudo systemctl daemon-reload
sudo systemctl enable parking-entry
sudo systemctl start parking-entry

# Проверить статус
sudo systemctl status parking-entry

# Логи
sudo journalctl -u parking-entry -f
```

### Вариант 3: Docker Container

#### Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/ParkingEntry.Api/ParkingEntry.Api.csproj", "ParkingEntry.Api/"]
COPY ["src/ParkingEntry.Core/ParkingEntry.Core.csproj", "ParkingEntry.Core/"]
RUN dotnet restore "ParkingEntry.Api/ParkingEntry.Api.csproj"
COPY src/ .
WORKDIR "/src/ParkingEntry.Api"
RUN dotnet build "ParkingEntry.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ParkingEntry.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ParkingEntry.Api.dll"]
```

#### Docker Compose

```yaml
# docker-compose.yml
version: '3.8'

services:
  parking-entry-api:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: parking-entry-api
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:5000
    restart: unless-stopped
    networks:
      - parking-network

networks:
  parking-network:
    driver: bridge
```

#### Команды

```bash
# Сборка и запуск
docker-compose up -d

# Остановка
docker-compose down

# Логи
docker-compose logs -f

# Пересборка после изменений
docker-compose up -d --build
```

---

## Configuration

### appsettings.json (Production)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Kestrel": {
    "Endpoints": {
      "Http": {
        "Url": "http://0.0.0.0:5000"
      }
    }
  },
  "EquipmentSettings": {
    "BarrierOpenTimeout": 5000,
    "BarrierCloseTimeout": 5000,
    "CardReadTimeout": 10000,
    "RetryCount": 3
  }
}
```

### Environment Variables

```bash
# Production
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS="http://0.0.0.0:5000"

# Database (если добавите)
export ConnectionStrings__DefaultConnection="Server=localhost;Database=Parking;..."

# Режим работы (Entry/Exit)
export RackMode="Entry"
```

---

## Reverse Proxy (Nginx)

### Для доступа через доменное имя

```nginx
# /etc/nginx/sites-available/parking-entry
server {
    listen 80;
    server_name parking-entry.local;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

```bash
# Активировать
sudo ln -s /etc/nginx/sites-available/parking-entry /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

---

## Мониторинг и логирование

### Структурированное логирование (Serilog)

Добавить пакет:
```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.File
```

В `Program.cs`:
```csharp
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/parking-entry-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
```

### Health Checks

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy());

app.MapHealthChecks("/health");
```

Проверка:
```bash
curl http://localhost:5000/health
```

---

## Интеграция с реальным оборудованием

### Замена Mock-сервисов

1. Создать реализации с ISlave/IMifareReader:

```csharp
public class HardwareBarrierService : IBarrierService
{
    private readonly ISlave _slave;
    
    public HardwareBarrierService(ISlave slave)
    {
        _slave = slave;
    }
    
    public async Task<BarrierStatus> OpenAsync(CancellationToken ct)
    {
        await _slave.SendCommandAsync("BARRIER_OPEN", ct);
        return BarrierStatus.Opened;
    }
}
```

2. Зарегистрировать в Program.cs:

```csharp
// Вместо Mock
builder.Services.AddSingleton<IBarrierService, HardwareBarrierService>();
builder.Services.AddSingleton<ISlave, SerialPortSlave>();
```

---

## Безопасность

### HTTPS в Production

```bash
# Генерация сертификата
dotnet dev-certs https --clean
dotnet dev-certs https -ep /path/to/cert.pfx -p YourPassword
```

```json
// appsettings.json
{
  "Kestrel": {
    "Endpoints": {
      "Https": {
        "Url": "https://0.0.0.0:5001",
        "Certificate": {
          "Path": "/path/to/cert.pfx",
          "Password": "YourPassword"
        }
      }
    }
  }
}
```

### API Key Authentication (опционально)

```csharp
// Middleware
public class ApiKeyMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKey))
        {
            context.Response.StatusCode = 401;
            return;
        }
        
        // Проверка ключа
        if (apiKey != "your-secret-key")
        {
            context.Response.StatusCode = 403;
            return;
        }
        
        await next(context);
    }
}
```

---

## Troubleshooting

### API не запускается

```bash
# Проверить порты
sudo netstat -tulpn | grep 5000

# Проверить права
ls -la /opt/parking-entry

# Проверить логи
sudo journalctl -u parking-entry -n 100
```

### Тесты падают

```bash
# Очистить и пересобрать
dotnet clean
dotnet restore
dotnet build
dotnet test
```

### Docker проблемы

```bash
# Очистить все
docker-compose down -v
docker system prune -a

# Пересоздать
docker-compose up -d --build
```

---

## Maintenance

### Обновление приложения

```bash
# 1. Остановить сервис
sudo systemctl stop parking-entry

# 2. Бэкап
sudo cp -r /opt/parking-entry /opt/parking-entry.backup

# 3. Обновить файлы
sudo cp -r publish/linux-arm64/* /opt/parking-entry/

# 4. Запустить
sudo systemctl start parking-entry

# 5. Проверить
sudo systemctl status parking-entry
```

### Логи

```bash
# Systemd
sudo journalctl -u parking-entry -f

# Docker
docker-compose logs -f

# Файлы (если используете Serilog)
tail -f logs/parking-entry-*.log
```

---

## Performance Tuning

### Для ARM64 терминалов

```json
// appsettings.json
{
  "Kestrel": {
    "Limits": {
      "MaxConcurrentConnections": 10,
      "MaxRequestBodySize": 1048576,
      "KeepAliveTimeout": "00:02:00"
    }
  }
}
```

### Memory optimization

```bash
# Limit memory для Docker
docker run -m 256m parking-entry-api
```

---

## Контакты и поддержка

При возникновении проблем:
1. Проверить логи
2. Проверить документацию
3. Создать issue в репозитории
