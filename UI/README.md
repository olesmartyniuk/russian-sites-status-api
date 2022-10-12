# Статус рашистських сайтів

Вебсайт показує поточний статус російських сайтів. Моніторинг відбувається кожні 300 сек з багатьох країн світу, включаючи росію (новосибірськ), за протоколом HTTP.

Це фронтенд частина для [Russian Sites Status API](https://github.com/olesmartyniuk/russian-sites-status-api), написана на TypeScript та [Angular 12](https://angular.io/).

## Початок роботи

Для роботи вам необхідно:

1. Встановити [NodeJS 16.15](https://nodejs.org/uk/blog/release/v16.15.0/)
2. Встановити TypeScript `npm install -g typescript`
3. Встановити Angular CLI `npm install -g @angular/cli`
4. Встановити [Git](https://git-scm.com/)
5. Клонувати репозиторій командою

`git clone https://github.com/olesmartyniuk/russian-sites-status-ui.git`

Для запуску веб програми локально виконайте наступні команди в склонованому репозиторії:

1. `npm install`
2. `npm start`

Ви повинні побачити наступне повідомлення, що свідчить про успішну компіляцію та запуск проєкту:

```Powershell
** Angular Live Development Server is listening on localhost:4200, open your browser on http://localhost:4200/ **
: Compiled successfully.
```

## Внесення змін в код

Перед початком попросіть власника додати вас як учасника до репозиторію.

1. Оберіть одну із задач, яка здається вам зрозумілою [зі списку](https://github.com/olesmartyniuk/russian-sites-status-ui/issues) і містить мітку `should be implemented`.
2. Створіть гілку в репозиторії з іменем задачі. (шаблон: ua-issue-суфікс. суфікс - це номер задачі.)
3. Напишіть код і протестуйте зміни локально.
4. Проштовхніть код на сервер і створіть Pull Request в основну гілку.
5. Після того, як Pull Requst буде злито в основну гілку, перевірте ваші зміни на бойовому сервері: http://www.mordor-sites-status.info/

## Додавання нових задач

Ви можете запропонувати новий функціонал чи прозвітувати про баг [на сторінці Issues](https://github.com/olesmartyniuk/russian-sites-status-ui/issues).
