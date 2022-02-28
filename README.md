# Статус рашистських сайтів

Web API, що дає доступ до статусу рашистських сайтів. Моніторинг відбувається кожні 60 сек з багатьох країн світу, включаючи росію (новосибірськ), за протоколом HTTP. 

Це бекенд частина частина для [вебсайту](https://github.com/olesmartyniuk/russian-sites-status-ui), написана на [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).

## Початок роботи

Для роботи вам необхідно:

1. Встановити [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
2. Встановити [Git](https://git-scm.com/) 
3. Клонувати репозиторій командою 

`git clone https://github.com/olesmartyniuk/russian-sites-status-api.git`

Для запуску веб програми локально виконайте наступні команди в склонованому репозиторії:
1. `dotnet build`
2. `dotnet run`

Ви повинні побачити наступне повідомлення, що свідчить про успішну компіляцію та запуск проєкту:
```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7160
```

## Внесення змін в код

Перед початком попросіть власника додати вас як учасника до репозиторію.

1. Оберіть одну із задач, яка здається вам зрозумілою [зі списку](https://github.com/olesmartyniuk/russian-sites-status-api/issues) і містить мітку `should be implemented`.
2. Створіть гілку в репозиторії з іменем задачі.
3. Напишіть код і протестуйте зміни локально.
4. Проштовхніть код на сервер і створіть Pull Request в основну гілку.
5. Після того, як Pull Requst буде злито в основну гілку, перевірте ваші зміни на бойовому сервері: https://russian-sites-status-api.herokuapp.com/

> Для тестування змін локально вам необхідно буде встановити StatusCake API Key. Попросіть автора надати цей ключ та інструкцію з його встановлення.

## Додавання нових задач

Ви можете запропонувати новий функціонал чи прозвітувати про баг [на сторінці Issues](https://github.com/olesmartyniuk/russian-sites-status-api/issues).
