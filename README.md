# Контрольная работа № 2 Синхронное межсервисное взаимодействие.

## Описание

Веб-приложение должно представлять функциональность «текстового сканера», который принимает на
вход отчёт студента (в формате .txt) и выполняет его анализ, включая:
1. Подсчёт статистики: количество абзацев; слов; символов. 
2. Сравнение файлов на схожесть (для выявления 100% плагиата среди ранее присланных отчетов). 
3. Визуализация данных (облака слов)

## Компоненты

1. **API Gateway**
    - Порт: **5003**
    - Отвечает за маршрутизацию запросов к бэкенд-сервисам.
    - Конфигурация: `ocelot.json`

2. **File Storing Service**
    - Порт: **5001**
    - Загрузка и хранение файлов в PostgreSQL.
    - Эндпоинты:
        - `POST /files` — **Body**: form-data, поле `file`
        - `GET  /files/{fileId}` — скачивание
    - Swagger UI: `http://localhost:5001/swagger`

3. **File Analysis Service**
    - Порт: **5002**
    - Анализ текстового содержимого, хранение результатов и генерация word-cloud.
    - Эндпоинты:
        - `POST /analysis/{fileId}` — запустить (или вернуть готовый) анализ
        - `GET  /analysis/{fileId}` — получить результат анализа
        - `GET  /analysis/{fileId}/cloud` — получить PNG-облако слов
    - Swagger UI: `http://localhost:5002/swagger`

4. **PostgreSQL**
    - Два отдельных контейнера (для файлов и для анализа).

## Документация
1. Postman Collection: [Ссылка на postman](https://vladimirgrigoryev-3319846.postman.co/workspace/Vladimir-Grigoryev's-Workspace~bad03bf3-a064-4005-8fe1-b011bf26544a/collection/43837745-21957484-f070-44d8-a903-13f7d9ad1459?action=share&creator=43837745)
2. Swagger:
    * FileStoring: `http://localhost:5001/swagger`
    * FileAnalysis: `http://localhost:5002/swagger`

## Быстрый старт

1. Клонировать репозиторий и перейти в папку проекта:
    `git clone <repo-url>`
    `cd <repo-folder>`
2. Запустить всё через Docker Compose:
   `docker-compose up --build`
3. Убедиться, что сервисы поднялись:
   `http://localhost:5003/health` → `{ "status": "Gateway is up" }`

## Использование (Postman)
### Все HTTP-запросы отправляются на API Gateway `(localhost:5003)`.
1. Загрузка файла 
   * Method: POST 
   * URL: `http://localhost:5003/files` 
   * Body: 
   * Тип: form-data 
   * Ключ: file (File) 
   * Значение: любой PDF/TXT/…
   * Пример можете увидеть в Postman коллекции
2. Method: GET
   * URL: `http://localhost:5003/files/{{fileId}}`
   * Params: fileId — UUID, полученный при загрузке.
3. Запуск анализа
   * Method: POST
   * URL: `http://localhost:5003/analysis/{{fileId}}`
   * Params: fileId — тот же UUID.
4. Получение результата анализа
   * Method: GET
   * URL: `http://localhost:5003/analysis/{{fileId}}`
   * Response:

   `{
       "id": "...",
       "fileId": "...",
       "fileHash": "...",
       "paragraphs": 5,
       "words": 120,
       "characters": 650,
       "similarityScore": 0.0,
       "createdAt": "2025-05-28T..."
   }`
5. Генерация word–cloud
   * Method: GET
   * URL: `http://localhost:5003/analysis/{{fileId}}/cloud`
   * Headers:
   * Accept: image/png

## Обработка ошибок

* 404 Not Found — файл или анализ не найдены. 
* 400 Bad Request — например, недостаточно текста для облака слов. 
* 502 Bad Gateway — внешний вызов (QuickChart или FileStoring) завершился ошибкой. 
* 500 Internal Server Error — непойманное исключение.
