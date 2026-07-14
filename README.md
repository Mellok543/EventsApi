# Events API

ASP.NET Core Web API для управления мероприятиями и бронированиями.

Проект использует .NET 8, хранит данные в памяти приложения и предоставляет Swagger UI для проверки API.

## Требования

- .NET 8 SDK или выше

## Запуск проекта

```bash
dotnet restore
dotnet build
dotnet run --project EventsApi
```

После запуска Swagger доступен по адресу, который будет выведен в консоль, например:

```text
https://localhost:5001/swagger
http://localhost:5000/swagger
```

## Тестирование

Юнит-тесты находятся в проекте `EventsApi.Tests` и запускаются из корня репозитория:

```bash
dotnet test
```

Тестами покрыты основные сценарии `EventService` и `BookingService`, включая создание брони, получение по ID, уникальность ID, изменение статуса, бронирование несуществующего или удалённого события.

## Модели

### Event

```json
{
  "id": "4e6c7b94-2e37-4c59-9c7e-0a4a8d5f7d12",
  "title": "Demo event",
  "description": "Example description",
  "startAt": "2026-07-18T10:00:00",
  "endAt": "2026-07-18T12:00:00"
}
```

### Booking

```json
{
  "id": "4cbd1840-0d43-4ac0-8da0-d04f781e6e1d",
  "eventId": "4e6c7b94-2e37-4c59-9c7e-0a4a8d5f7d12",
  "status": "Pending",
  "createdAt": "2026-07-14T15:30:00Z",
  "processedAt": null
}
```

Статусы бронирования:

- `Pending` — бронь создана и ожидает фоновой обработки.
- `Confirmed` — бронь подтверждена.
- `Rejected` — бронь отклонена.

При создании бронь получает уникальный `Id`, статус `Pending` и текущую дату в `CreatedAt`.

## Endpoints

### Получить список мероприятий

```http
GET /events
```

Поддерживает фильтрацию и пагинацию через query-параметры.

| Параметр | Тип | По умолчанию | Описание |
| --- | --- | --- | --- |
| `title` | `string` | — | Поиск по названию, без учёта регистра, по частичному совпадению. |
| `from` | `DateTime` | — | Вернуть события, которые начинаются не раньше указанной даты (`StartAt >= from`). |
| `to` | `DateTime` | — | Вернуть события, которые заканчиваются не позже указанной даты (`EndAt <= to`). |
| `page` | `int` | `1` | Номер страницы. |
| `pageSize` | `int` | `10` | Количество элементов на странице. |

Примеры:

```http
GET /events?title=conference
GET /events?from=2026-07-01&to=2026-07-31
GET /events?page=2&pageSize=5
```

### Получить мероприятие по ID

```http
GET /events/{id}
```

Возвращает `200 OK`, если мероприятие найдено. Если мероприятие не найдено, возвращает `404 Not Found`.

### Создать мероприятие

```http
POST /events
```

Тело запроса:

```json
{
  "title": "Demo event",
  "description": "Example description",
  "startAt": "2026-07-18T10:00:00",
  "endAt": "2026-07-18T12:00:00"
}
```

При успешном создании возвращает `201 Created`.

### Обновить мероприятие

```http
PUT /events/{id}
```

Тело запроса:

```json
{
  "title": "Updated event",
  "description": "Updated description",
  "startAt": "2026-07-18T13:00:00",
  "endAt": "2026-07-18T15:00:00"
}
```

Возвращает `200 OK`, если мероприятие найдено. Если мероприятие не найдено, возвращает `404 Not Found`.

### Удалить мероприятие

```http
DELETE /events/{id}
```

Возвращает `204 No Content`, если мероприятие найдено и удалено. Если мероприятие не найдено, возвращает `404 Not Found`.

### Создать бронь для мероприятия

```http
POST /events/{id}/book
```

Создаёт бронь для указанного мероприятия и сразу возвращает быстрый ответ `202 Accepted`.

В ответе:

- тело содержит созданную бронь со статусом `Pending`;
- заголовок `Location` содержит ссылку на ресурс брони, например `/bookings/{bookingId}`;
- если мероприятие не найдено, возвращается `404 Not Found`.

Пример ответа:

```json
{
  "id": "4cbd1840-0d43-4ac0-8da0-d04f781e6e1d",
  "eventId": "4e6c7b94-2e37-4c59-9c7e-0a4a8d5f7d12",
  "status": "Pending",
  "createdAt": "2026-07-14T15:30:00Z",
  "processedAt": null
}
```

### Получить бронь по ID

```http
GET /bookings/{id}
```

Возвращает текущее состояние брони:

- `200 OK`, если бронь найдена;
- `404 Not Found`, если бронь не найдена.

## Фоновая обработка бронирований

Фоновая обработка реализована через `BackgroundService`.

Сервис периодически:

1. Получает брони в статусе `Pending`.
2. Для каждой брони выполняет искусственную задержку 2 секунды, имитируя обращение к внешней системе.
3. Повторно проверяет текущее состояние брони.
4. Если бронь всё ещё `Pending`, переводит её в `Confirmed`.
5. Заполняет поле `ProcessedAt`.

Фоновый сервис зарегистрирован через `AddHostedService`.

## Пример сценария

1. Создать мероприятие:

```http
POST /events
```

```json
{
  "title": "Demo event",
  "description": "Example description",
  "startAt": "2026-07-18T10:00:00",
  "endAt": "2026-07-18T12:00:00"
}
```

2. Создать бронь:

```http
POST /events/{eventId}/book
```

Ответ: `202 Accepted`, статус брони — `Pending`, заголовок `Location` указывает на `/bookings/{bookingId}`.

3. Сразу запросить бронь:

```http
GET /bookings/{bookingId}
```

Статус будет `Pending`.

4. Подождать несколько секунд и повторить запрос:

```http
GET /bookings/{bookingId}
```

Статус будет `Confirmed`, поле `ProcessedAt` будет заполнено.

## Обработка ошибок

Все необработанные исключения перехватываются глобальным middleware `ExceptionHandlingMiddleware`, который возвращает JSON в формате Problem Details.

| Ситуация | HTTP-статус |
| --- | --- |
| Ошибка валидации бизнес-логики | `400 Bad Request` |
| Ресурс не найден | `404 Not Found` |
| Непредвиденная ошибка | `500 Internal Server Error` |

Пример `404 Not Found`:

```json
{
  "title": "Not Found",
  "status": 404,
  "detail": "Booking with id '11111111-2222-3333-4444-555555555555' was not found",
  "instance": "/bookings/11111111-2222-3333-4444-555555555555"
}
```
