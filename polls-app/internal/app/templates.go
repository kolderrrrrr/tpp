package app

const pagesHTML = `
{{define "layoutStart"}}
<!doctype html>
<html lang="ru">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>{{.Title}}</title>
  <link rel="stylesheet" href="/static/styles.css">
</head>
<body>
  <header class="topbar">
    <a class="brand" href="/">PollsApp</a>
    <nav>
      {{if .User}}
        <a href="/polls">Мои опросы</a>
        <span>{{.User.Name}}</span>
        <a href="/logout">Выйти</a>
      {{else}}
        <a href="/login">Войти</a>
        <a class="button small" href="/register">Регистрация</a>
      {{end}}
    </nav>
  </header>
  <main class="shell">
    {{if .Error}}<div class="alert error">{{.Error}}</div>{{end}}
    {{if .Success}}<div class="alert success">{{.Success}}</div>{{end}}
{{end}}

{{define "layoutEnd"}}
  </main>
</body>
</html>
{{end}}

{{define "home"}}
{{template "layoutStart" .}}
<section class="hero">
  <div>
    <h1>Создавайте опросы и анализируйте ответы</h1>
    <p>Минимальная версия курсового веб-приложения на Go: авторизация, создание опросов, публичное голосование и результаты.</p>
    <div class="actions">
      <a class="button" href="/register">Начать работу</a>
      <a class="button secondary" href="/login">Войти</a>
    </div>
  </div>
</section>
{{template "layoutEnd" .}}
{{end}}

{{define "register"}}
{{template "layoutStart" .}}
<section class="panel auth">
  <h1>Регистрация</h1>
  <form method="post" action="/register">
    <label>Имя <input name="name" autocomplete="name" required></label>
    <label>Email <input name="email" type="email" autocomplete="email" required></label>
    <label>Пароль <input name="password" type="password" autocomplete="new-password" minlength="6" required></label>
    <button class="button" type="submit">Создать аккаунт</button>
  </form>
</section>
{{template "layoutEnd" .}}
{{end}}

{{define "login"}}
{{template "layoutStart" .}}
<section class="panel auth">
  <h1>Вход</h1>
  <form method="post" action="/login">
    <label>Email <input name="email" type="email" autocomplete="email" required></label>
    <label>Пароль <input name="password" type="password" autocomplete="current-password" required></label>
    <button class="button" type="submit">Войти</button>
  </form>
</section>
{{template "layoutEnd" .}}
{{end}}

{{define "polls"}}
{{template "layoutStart" .}}
<div class="grid two">
  <section>
    <div class="section-head">
      <h1>Мои опросы</h1>
    </div>
    {{if .Polls}}
      <div class="list">
        {{range .Polls}}
          <article class="item">
            <div>
              <h2><a href="/polls/{{.ID}}">{{.Title}}</a></h2>
              <p>{{.Description}}</p>
              <small>{{if .Published}}Опубликован: /p/{{.ID}}{{else}}Черновик{{end}}</small>
            </div>
            <a class="button secondary small" href="/polls/{{.ID}}">Открыть</a>
          </article>
        {{end}}
      </div>
    {{else}}
      <p class="muted">Пока нет опросов. Создайте первый справа.</p>
    {{end}}
  </section>
  <section class="panel">
    <h2>Новый опрос</h2>
    <form method="post" action="/polls">
      <label>Название <input name="title" required></label>
      <label>Описание <textarea name="description" rows="4"></textarea></label>
      <button class="button" type="submit">Создать</button>
    </form>
  </section>
</div>
{{template "layoutEnd" .}}
{{end}}

{{define "poll"}}
{{template "layoutStart" .}}
<section class="title-row">
  <div>
    <h1>{{.Poll.Title}}</h1>
    <p>{{.Poll.Description}}</p>
    <small>{{if .Poll.Published}}Публичная ссылка: <a href="/p/{{.Poll.ID}}">/p/{{.Poll.ID}}</a>{{else}}Черновик{{end}}</small>
  </div>
  <div class="actions">
    {{if .Poll.Published}}
      <a class="button secondary" href="/polls/{{.Poll.ID}}/results">Результаты</a>
      <a class="button" href="/p/{{.Poll.ID}}">Пройти</a>
    {{else}}
      <form method="post" action="/polls/{{.Poll.ID}}/publish"><button class="button" type="submit">Опубликовать</button></form>
    {{end}}
    <form method="post" action="/polls/{{.Poll.ID}}/delete"><button class="button danger" type="submit">Удалить</button></form>
  </div>
</section>

<div class="grid two">
  <section>
    <h2>Вопросы</h2>
    {{if .Poll.Questions}}
      <div class="list">
        {{range .Poll.Questions}}
          <article class="item block">
            <h3>{{.Text}}</h3>
            <ol>
              {{range .Options}}<li>{{.Text}}</li>{{end}}
            </ol>
          </article>
        {{end}}
      </div>
    {{else}}
      <p class="muted">Добавьте хотя бы один вопрос с вариантами ответа.</p>
    {{end}}
  </section>
  <section class="panel">
    <h2>Добавить вопрос</h2>
    <form method="post" action="/polls/{{.Poll.ID}}/questions">
      <label>Текст вопроса <input name="text" required></label>
      <label>Варианты ответа, каждый с новой строки <textarea name="options" rows="6" required></textarea></label>
      <button class="button" type="submit">Добавить</button>
    </form>
  </section>
</div>
{{template "layoutEnd" .}}
{{end}}

{{define "publicPoll"}}
{{template "layoutStart" .}}
<section class="panel wide">
  <h1>{{.Poll.Title}}</h1>
  <p>{{.Poll.Description}}</p>
  {{if .Voted}}
    <p class="muted">Ваш ответ уже учтен.</p>
  {{else}}
    <form method="post" action="/p/{{.Poll.ID}}">
      {{range .Poll.Questions}}
        {{$questionID := .ID}}
        <fieldset>
          <legend>{{.Text}}</legend>
          {{range .Options}}
            <label class="choice"><input type="radio" name="q_{{$questionID}}" value="{{.ID}}" required> {{.Text}}</label>
          {{end}}
        </fieldset>
      {{end}}
      <button class="button" type="submit">Отправить ответы</button>
    </form>
  {{end}}
</section>
{{template "layoutEnd" .}}
{{end}}

{{define "results"}}
{{template "layoutStart" .}}
<section class="title-row">
  <div>
    <h1>Результаты: {{.Result.Poll.Title}}</h1>
    <p>Всего ответов: {{.Result.Total}}</p>
  </div>
  <a class="button secondary" href="/polls/{{.Result.Poll.ID}}">К опросу</a>
</section>
{{range .Result.Questions}}
  <section class="panel wide">
    <h2>{{.Text}}</h2>
    {{range .Options}}
      <div class="result-row">
        <div class="result-label"><span>{{.Text}}</span><strong>{{.Count}} / {{.Percent}}%</strong></div>
        <div class="bar"><span style="width: {{.Percent}}%"></span></div>
      </div>
    {{end}}
  </section>
{{end}}
{{template "layoutEnd" .}}
{{end}}
`
