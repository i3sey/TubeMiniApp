# 🚀 TubeMiniApp - Telegram Mini App для трубной металлургической компании

**Проект создан в рамках хакатона РадиоХак 2.0**

Веб-приложение для заказа трубной продукции через Telegram Mini App с возможностью фильтрации, управления корзиной и оформления заказов.

## 📱 Описание проекта

TubeMiniApp - это современное веб-приложение, интегрированное с Telegram, которое предоставляет удобный интерфейс для:

- 🔍 **Поиска и фильтрации трубной продукции** по параметрам (диаметр, толщина стенки, ГОСТ, марка стали, склад)
- 🛒 **Управления корзиной** с автоматическим расчетом количества в тоннах и метрах
- 💰 **Системы скидок** в зависимости от объема заказа
- 📋 **Оформления заказов** с уведомлениями в Telegram
- 👤 **Профиля пользователя** с историей заказов

## 🏗️ Архитектура проекта

```
TubeMiniApp/
├── TubeMiniApp.API/              # ASP.NET Core Web API (.NET 8)
│   ├── Controllers/              # API контроллеры
│   │   ├── ProductsController    # Управление продукцией и фильтры
│   │   ├── CartController        # Корзина покупок
│   │   ├── OrdersController      # Заказы
│   │   ├── DiscountsController   # Система скидок
│   │   ├── TelegramController    # Интеграция с Telegram
│   │   └── HealthController      # Health checks
│   ├── Services/                 # Бизнес-логика
│   ├── Models/                   # Модели данных (Product, Cart, Order)
│   ├── DTOs/                     # Data Transfer Objects
│   ├── Data/                     # Entity Framework DbContext
│   └── Middleware/               # Telegram авторизация
├── frontend/                     # React TypeScript SPA
│   ├── src/
│   │   ├── components/           # React компоненты
│   │   ├── api.ts               # API клиент
│   │   ├── types.ts             # TypeScript типы
│   │   └── store.ts             # Zustand store
│   └── public/
├── nginx/                        # Nginx reverse proxy
├── TubeMiniApp.Tests/           # Unit тесты
└── docker-compose.yml          # Docker оркестрация
```

## 🛠️ Технологический стек

### Backend (TubeMiniApp.API)
- **ASP.NET Core 8.0** - основной фреймворк
- **Entity Framework Core** - ORM с поддержкой InMemory и SQL Server
- **Swagger/OpenAPI** - документация API
- **FluentValidation** - валидация данных
- **Serilog** - логирование

### Frontend
- **React 18** с **TypeScript** - пользовательский интерфейс
- **Zustand** - управление состоянием
- **React Router DOM** - маршрутизация
- **Telegram WebApp SDK** - интеграция с Telegram
- **Lucide React** - иконки

### DevOps
- **Docker & Docker Compose** - контейнеризация
- **Nginx** - reverse proxy с gzip сжатием и rate limiting
- **Multi-stage builds** - оптимизация размера образов

## 🚀 Быстрый старт

### Предварительные требования
- Docker и Docker Compose
- (Опционально) .NET 8 SDK и Node.js 18+ для разработки

### 1. Клонирование репозитория
```bash
git clone https://github.com/i3sey/TubeMiniApp.git
cd TubeMiniApp
```

### 2. Настройка переменных окружения
Создайте файл `.env` в корне проекта:
```env
# Telegram Bot Token (получить у @BotFather)
TELEGRAM_BOT_TOKEN=your_bot_token_here

# База данных (по умолчанию используется InMemory)
CONNECTION_STRING=UseInMemory
# Для SQL Server: CONNECTION_STRING=Server=localhost;Database=TubeMiniApp;Trusted_Connection=true;

# Окружение
ASPNETCORE_ENVIRONMENT=Production
```

### 3. Запуск через Docker Compose
```bash
# Сборка и запуск всех сервисов
docker-compose up --build

# Или в фоновом режиме
docker-compose up -d --build
```

### 4. Проверка работы
- **API**: http://localhost/api/health
- **Swagger документация**: http://localhost/swagger
- **Frontend**: http://localhost



## 📊 Основной функционал

### 🔍 Продукция и фильтры
- Поиск по параметрам трубы (диаметр, толщина стенки)
- Фильтрация по складу, типу продукции, ГОСТ, марке стали
- Пагинация результатов
- Отображение цены за тонну и остатков на складе

### 🛒 Корзина
- Добавление товаров с указанием количества в метрах
- Автоматический расчет веса в тоннах
- Применение скидок по объему
- Обновление и удаление позиций

### 💰 Система скидок
- Скидки от 5% при заказе от 100 кг
- Скидки от 10% при заказе от 500 кг  
- Скидки от 15% при заказе от 1 тонны
- Возможность настройки скидок по типу продукции и складу

### 📱 Telegram интеграция
- Аутентификация через Telegram WebApp
- Уведомления о новых заказах в Telegram
- Адаптивный интерфейс для мобильных устройств

## 🗃️ Структура базы данных

### Основные таблицы:
- **Products** - каталог трубной продукции
- **Carts** - корзины пользователей
- **CartItems** - позиции в корзине
- **Orders** - заказы
- **OrderItems** - позиции заказа
- **Discounts** - настройки скидок

## 🔐 Безопасность

- Rate limiting (100 запросов в минуту)
- Валидация Telegram WebApp данных
- CORS настройки для Telegram
- Health checks для мониторинга

## 📋 API Endpoints

### Продукция
- `GET /api/products` - список продукции с фильтрами
- `GET /api/products/{id}` - продукт по ID
- `GET /api/products/warehouses` - список складов
- `GET /api/products/filter-options` - опции для фильтров

### Корзина
- `GET /api/cart/{telegramUserId}` - получить корзину
- `POST /api/cart` - добавить в корзину
- `PUT /api/cart/item/{itemId}` - обновить количество
- `DELETE /api/cart/item/{itemId}` - удалить позицию

### Заказы
- `POST /api/orders` - создать заказ
- `GET /api/orders/user/{telegramUserId}` - заказы пользователя

### Скидки
- `GET /api/discounts` - активные скидки

## 🐳 Docker конфигурация

Проект использует multi-container архитектуру:
- **API** - ASP.NET Core приложение
- **Frontend** - React SPA с nginx
- **Nginx** - reverse proxy с SSL termination

## Переход с InMemory на SQL Server

По умолчанию приложение использует InMemory базу данных для демонстрации. Для продакшена рекомендуется перейти на SQL Server.

### Шаги для перехода:

1. **Обновите Program.cs:**
```csharp
// Замените строку в TubeMiniApp.API/Program.cs
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
```

2. **Добавьте строку подключения в appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TubeMiniAppDb;Trusted_Connection=true;"
  },
  "Logging": {
    // ...existing config...
  }
}
```

3. **Создайте и примените миграции:**
```bash
cd TubeMiniApp.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

4. **Для Docker с SQL Server добавьте в docker-compose.yml:**
```yaml
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      SA_PASSWORD: "YourPassword123!"
      ACCEPT_EULA: "Y"
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql

  api:
    # ...existing config...
    environment:
      - ConnectionStrings__DefaultConnection=Server=sqlserver;Database=TubeMiniAppDb;User Id=sa;Password=YourPassword123!;TrustServerCertificate=true;
    depends_on:
      - sqlserver

volumes:
  sqlserver_data:
```

### Преимущества SQL Server:
- ✅ Данные сохраняются после перезапуска
- ✅ Поддержка транзакций и целостности данных
- ✅ Лучшая производительность для больших объемов
- ✅ Готовность к продакшену

