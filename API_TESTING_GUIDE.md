# API Testing Examples

## Postman Collection

Импортируйте эту коллекцию в Postman для тестирования API.

### Base URL
```
https://localhost:5001
```

---

## 1. Получить текущий статус

**GET** `/api/entry/status`

**Response:**
```json
{
  "currentState": "Idle",
  "cardNumber": null,
  "timestamp": "2025-01-15T10:00:00Z"
}
```

---

## 2. Сценарий: Успешный въезд

### Шаг 1: Автомобиль подъезжает

**POST** `/api/entry/events/vehicle-approached`

**Response:**
```json
{
  "message": "Event published",
  "state": "ReadingCard"
}
```

### Шаг 2: Карта прочитана

**POST** `/api/entry/events/card-read`

**Headers:**
```
Content-Type: application/json
```

**Body:**
```json
{
  "cardNumber": "CARD12345"
}
```

**Response:**
```json
{
  "message": "Event published",
  "state": "WaitingPassage"
}
```

### Шаг 3: Автомобиль проехал

**POST** `/api/entry/events/vehicle-passed`

**Response:**
```json
{
  "message": "Event published",
  "state": "Idle"
}
```

### Проверка финального состояния

**GET** `/api/entry/status`

**Response:**
```json
{
  "currentState": "Idle",
  "cardNumber": null,
  "timestamp": "2025-01-15T10:05:00Z"
}
```

---

## 3. Сценарий: Отказ в доступе

### Шаг 1: Автомобиль подъезжает

**POST** `/api/entry/events/vehicle-approached`

### Шаг 2: Карта отклонена

**POST** `/api/entry/events/card-read`

**Body:**
```json
{
  "cardNumber": "DENIED"
}
```

**Response:**
```json
{
  "message": "Event published",
  "state": "AccessDenied"
}
```

### Шаг 3: Автомобиль уезжает

**POST** `/api/entry/events/vehicle-left`

**Response:**
```json
{
  "message": "Event published",
  "state": "Idle"
}
```

---

## 4. Сценарий: Автомобиль уехал назад

### Шаг 1-2: Подъезд и чтение карты
(Как в успешном въезде)

### Шаг 3: Автомобиль уехал назад

**POST** `/api/entry/events/vehicle-reversed`

**Response:**
```json
{
  "message": "Event published",
  "state": "Idle"
}
```

---

## 5. Сценарий: Автомобиль уехал до чтения карты

### Шаг 1: Автомобиль подъезжает

**POST** `/api/entry/events/vehicle-approached`

### Шаг 2: Автомобиль уезжает (без чтения карты)

**POST** `/api/entry/events/vehicle-left`

**Response:**
```json
{
  "message": "Event published",
  "state": "Idle"
}
```

---

## 6. Сброс системы

**POST** `/api/entry/reset`

**Response:**
```json
{
  "message": "System reset",
  "state": "Idle"
}
```

---

## cURL Examples

### Успешный въезд (полная последовательность)

```bash
#!/bin/bash

API_URL="https://localhost:5001/api/entry"

echo "1. Проверка начального состояния..."
curl -k -X GET "${API_URL}/status"

echo -e "\n\n2. Автомобиль подъезжает..."
curl -k -X POST "${API_URL}/events/vehicle-approached"

echo -e "\n\n3. Карта прочитана..."
curl -k -X POST "${API_URL}/events/card-read" \
  -H "Content-Type: application/json" \
  -d '{"cardNumber": "VALID_CARD_123"}'

echo -e "\n\n4. Проверка состояния WaitingPassage..."
curl -k -X GET "${API_URL}/status"

echo -e "\n\n5. Автомобиль проехал..."
curl -k -X POST "${API_URL}/events/vehicle-passed"

echo -e "\n\n6. Проверка финального состояния..."
curl -k -X GET "${API_URL}/status"
```

### Отказ в доступе

```bash
#!/bin/bash

API_URL="https://localhost:5001/api/entry"

curl -k -X POST "${API_URL}/reset"
curl -k -X POST "${API_URL}/events/vehicle-approached"
curl -k -X POST "${API_URL}/events/card-read" \
  -H "Content-Type: application/json" \
  -d '{"cardNumber": "DENIED"}'
curl -k -X GET "${API_URL}/status"
curl -k -X POST "${API_URL}/events/vehicle-left"
```

---

## PowerShell Examples

### Успешный въезд

```powershell
$baseUrl = "https://localhost:5001/api/entry"

# Игнорируем SSL ошибки для локального тестирования
[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

# 1. Сброс
Invoke-RestMethod -Uri "$baseUrl/reset" -Method Post

# 2. Автомобиль подъезжает
Invoke-RestMethod -Uri "$baseUrl/events/vehicle-approached" -Method Post

# 3. Карта прочитана
$body = @{ cardNumber = "CARD_001" } | ConvertTo-Json
Invoke-RestMethod -Uri "$baseUrl/events/card-read" -Method Post -Body $body -ContentType "application/json"

# 4. Автомобиль проехал
Invoke-RestMethod -Uri "$baseUrl/events/vehicle-passed" -Method Post

# 5. Проверка статуса
Invoke-RestMethod -Uri "$baseUrl/status" -Method Get
```

---

## Python Examples

### Успешный въезд

```python
import requests
import json

# Игнорируем SSL предупреждения
requests.packages.urllib3.disable_warnings()

base_url = "https://localhost:5001/api/entry"

# 1. Сброс
response = requests.post(f"{base_url}/reset", verify=False)
print("Reset:", response.json())

# 2. Автомобиль подъезжает
response = requests.post(f"{base_url}/events/vehicle-approached", verify=False)
print("Vehicle approached:", response.json())

# 3. Карта прочитана
card_data = {"cardNumber": "PYTHON_CARD"}
response = requests.post(
    f"{base_url}/events/card-read",
    json=card_data,
    verify=False
)
print("Card read:", response.json())

# 4. Автомобиль проехал
response = requests.post(f"{base_url}/events/vehicle-passed", verify=False)
print("Vehicle passed:", response.json())

# 5. Проверка статуса
response = requests.get(f"{base_url}/status", verify=False)
print("Final status:", response.json())
```

---

## Swagger UI

Откройте в браузере:
```
https://localhost:5001/swagger
```

Swagger предоставляет интерактивный UI для тестирования всех endpoint'ов.

---

## Мониторинг логов

При запуске API все события логируются в консоль:

```
info: ParkingEntry.Api.Controllers.EntryController[0]
      Получено событие: VehicleApproached
info: ParkingEntry.Core.Application.Handlers.VehicleApproachedHandler[0]
      Автомобиль подъехал. Переход в состояние ReadingCard
info: ParkingEntry.Api.Services.MockLightService[0]
      Переключаем светофор на Red
info: ParkingEntry.Api.Services.MockMifareService[0]
      Начинаем поиск карты
```

---

## Troubleshooting

### Ошибка SSL Certificate

Если получаете ошибку SSL:
- В cURL: добавьте флаг `-k` или `--insecure`
- В Postman: Settings → SSL certificate verification → OFF
- В браузере: примите самоподписанный сертификат

### Порт занят

Если порт 5001 занят, измените в `launchSettings.json` или используйте:
```bash
dotnet run --urls "https://localhost:5555"
```
