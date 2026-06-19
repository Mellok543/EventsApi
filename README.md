# Events API

ASP.NET Core Web API для управления мероприятиями.

## Требования

* .NET 8 SDK или выше

## Запуск проекта

```bash
dotnet restore
dotnet build
dotnet run
```

После запуска Swagger будет доступен по адресу:

```text
https://localhost:5001/swagger
```

или

```text
http://localhost:5000/swagger
```

Точный адрес будет выведен в консоли после запуска приложения.

## Описание API

### Получить все мероприятия

```http
GET /events
```

Ответ:

```json
[
  {
    "id": "4e6c7b94-2e37-4c59-9c7e-0a4a8d5f7d12",
    "title": "Demo event",
    "description": "Example description",
    "startAt": "2026-06-18T10:00:00",
    "endAt": "2026-06-18T12:00:00"
  }
]
```

### Получить мероприятие по ID

```http
GET /events/{id}
```

Если мероприятие найдено, возвращается `200 OK`.

Если мероприятие не найдено, возвращается `404 Not Found`.

### Создать мероприятие

```http
POST /events
```

Тело запроса:

```json
{
  "title": "Demo event",
  "description": "Example description",
  "startAt": "2026-06-18T10:00:00",
  "endAt": "2026-06-18T12:00:00"
}
```

При успешном создании возвращается `201 Created`.

### Обновить мероприятие

```http
PUT /events/{id}
```

Тело запроса:

```json
{
  "title": "Updated event",
  "description": "Updated description",
  "startAt": "2026-06-18T13:00:00",
  "endAt": "2026-06-18T15:00:00"
}
```

Если мероприятие найдено, возвращается `200 OK`.

Если мероприятие не найдено, возвращается `404 Not Found`.

### Удалить мероприятие

```http
DELETE /events/{id}
```

Если мероприятие найдено и удалено, возвращается `204 No Content`.

Если мероприятие не найдено, возвращается `404 Not Found`.

## Валидация

Поля `title`, `startAt`, `endAt` обязательны.

Поле `endAt` должно быть позже `startAt`.

Если данные не проходят валидацию, API возвращает `400 Bad Request`.

