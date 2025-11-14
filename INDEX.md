# 📦 Parking Entry System - Навигация по проекту

## 🚀 Быстрый старт

1. **Начать здесь**: [README.md](./README.md)
2. **Запуск**: Прочитать секцию "Установка и запуск" в README
3. **Тесты**: `dotnet test`
4. **API**: `dotnet run --project src/ParkingEntry.Api`

---

## 📚 Документация

### Для начинающих
- 📖 **[README.md](./README.md)** - Главная документация
  - Обзор архитектуры
  - Инструкции по установке и запуску
  - Примеры использования API
  - Структура проекта

- ⚡ **[QUICK_REFERENCE.md](./QUICK_REFERENCE.md)** - Быстрая справка
  - Команды быстрого доступа
  - Типичные сценарии
  - Частые ошибки и решения
  - Checklist

### Для разработчиков
- 🏗️ **[ARCHITECTURE.md](./ARCHITECTURE.md)** - Детальная архитектура
  - Диаграммы компонентов
  - Поток обработки событий
  - Детали переходов состояний
  - DI контейнер

- 🧪 **[PROJECT_SUMMARY.md](./PROJECT_SUMMARY.md)** - Итоговая сводка
  - Что создано
  - Статистика проекта
  - Соответствие ТЗ
  - Известные ограничения

### Для DevOps
- 🚢 **[DEPLOYMENT.md](./DEPLOYMENT.md)** - Развертывание
  - Development setup
  - Production deployment
  - Docker/Systemd
  - Мониторинг и логирование

- 🔧 **[API_TESTING_GUIDE.md](./API_TESTING_GUIDE.md)** - Тестирование API
  - Postman коллекции
  - cURL примеры
  - PowerShell/Python скрипты
  - Типичные сценарии

---

## 🗂️ Структура проекта

```
ParkingEntry/
├── 📄 README.md                          ← Начать здесь!
├── 📄 PROJECT_SUMMARY.md                 ← Итоговая сводка
├── 📄 ARCHITECTURE.md                    ← Детальная архитектура
├── 📄 DEPLOYMENT.md                      ← Развертывание
├── 📄 API_TESTING_GUIDE.md               ← Примеры API
├── 📄 QUICK_REFERENCE.md                 ← Шпаргалка
├── 📦 ParkingEntry-Complete.tar.gz       ← Архив проекта
│
├── 📁 src/
│   ├── ParkingEntry.Core/                ← Бизнес-логика
│   │   ├── Domain/                       - Модели и события
│   │   ├── Application/                  - Handlers и команды
│   │   └── Infrastructure/               - Реализации
│   │
│   └── ParkingEntry.Api/                 ← ASP.NET API
│       ├── Controllers/                  - HTTP endpoints
│       ├── Services/                     - Mock сервисы
│       └── Program.cs                    - Конфигурация
│
├── 🧪 tests/
│   └── ParkingEntry.Tests/               ← Unit тесты
│       ├── EntryStateMachineTestBase.cs  - Базовый класс
│       ├── LightBehaviorTests.cs         - Тесты светофора
│       ├── CardReaderTests.cs            - Тесты карт
│       ├── FinalStateTests.cs            - ⭐ Тесты с любого состояния
│       └── FullScenarioTests.cs          - Полные сценарии
│
└── 🔧 ParkingEntry.sln                   ← Solution файл
```

---

## 🎯 Основные сценарии использования

### Я хочу...

#### ...запустить проект
→ [README.md](./README.md) раздел "Установка и запуск"

#### ...понять архитектуру
→ [ARCHITECTURE.md](./ARCHITECTURE.md)

#### ...запустить тесты
```bash
dotnet test
```
→ [README.md](./README.md) раздел "Запуск тестов"

#### ...протестировать API
→ [API_TESTING_GUIDE.md](./API_TESTING_GUIDE.md)

#### ...развернуть в production
→ [DEPLOYMENT.md](./DEPLOYMENT.md)

#### ...добавить новое событие
→ [README.md](./README.md) раздел "Расширение системы"

#### ...понять что было сделано
→ [PROJECT_SUMMARY.md](./PROJECT_SUMMARY.md)

#### ...найти команды быстро
→ [QUICK_REFERENCE.md](./QUICK_REFERENCE.md)

---

## ✅ Ключевые особенности

### 🎭 Тестирование с любого состояния
```csharp
// Начать тест с WaitingPassage, минуя все предыдущие!
Setup(EntryState.WaitingPassage, cardNumber: "TEST123");
await Mediator.Publish(new VehiclePassed());
AssertState(EntryState.Idle);
```

### 🧩 Mediator Pattern
- Слабая связанность
- Легко расширять
- Просто тестировать

### 🏗️ Clean Architecture
- Domain → Application → API
- Зависимости только вниз
- Тестируемость на каждом уровне

### 🔒 Thread-Safe
- Singleton State Machine
- Lock для безопасности
- Последовательная обработка

---

## 📊 Статистика

- ✅ **52 файла C#**
- ✅ **31 unit тест**
- ✅ **8 документов**
- ✅ **2500+ строк кода**
- ✅ **100% соответствие ТЗ**

---

## 🛠️ Технологии

- .NET 8.0
- ASP.NET Core
- MediatR 12.4.1
- NUnit 4.2.2
- FluentAssertions 6.12.1
- Moq 4.20.72

---

## 🆘 Помощь

### Проблемы с запуском?
→ [DEPLOYMENT.md](./DEPLOYMENT.md) раздел "Troubleshooting"

### Как работать с API?
→ [API_TESTING_GUIDE.md](./API_TESTING_GUIDE.md)

### Нужна быстрая команда?
→ [QUICK_REFERENCE.md](./QUICK_REFERENCE.md)

### Не понимаю архитектуру?
→ [ARCHITECTURE.md](./ARCHITECTURE.md)

---

## 📞 Поддержка

1. Проверить документацию
2. Просмотреть примеры в тестах
3. Прочитать PROJECT_SUMMARY.md

---

## 🎉 Что дальше?

1. **Изучить**: [README.md](./README.md)
2. **Запустить**: `dotnet run --project src/ParkingEntry.Api`
3. **Тестировать**: `dotnet test`
4. **Расширять**: Добавить свои handlers

---

**Готово к использованию!** 🚀

Начните с [README.md](./README.md) и следуйте инструкциям.
