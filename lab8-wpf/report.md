# Отчет по лабораторной работе 8

## Тема

Разработка многоязычного WPF-приложения с использованием ресурсных файлов `.resx`.

## Цель работы

Овладеть принципами создания многоязычных приложений на платформе WPF и освоить механизмы локализации интерфейсов.

## Краткое описание проекта

В ходе лабораторной работы было создано WPF-приложение `Многоязычный блокнот`. Приложение позволяет создавать новый документ, открывать текстовые файлы, сохранять текст в файл и закрывать окно с подтверждением при наличии несохраненных изменений.

Интерфейс поддерживает русский и английский языки. Переключение языка выполняется через выпадающий список без перезапуска приложения.

## Использованные технологии

В работе использовались:

- Microsoft Visual Studio;
- платформа .NET;
- технология Windows Presentation Foundation;
- язык разметки XAML;
- язык программирования C#;
- ресурсные файлы `.resx`;
- классы `OpenFileDialog` и `SaveFileDialog`;
- файловый ввод-вывод.

## Интерфейс приложения

Главное окно содержит меню для работы с документом, выпадающий список выбора языка, многострочное текстовое поле и строку состояния.

Фрагмент меню:

```xml
<MenuItem x:Name="FileMenuItem" Header="_File">
    <MenuItem x:Name="NewMenuItem" Click="NewMenuItem_Click" />
    <MenuItem x:Name="OpenMenuItem" Click="OpenMenuItem_Click" />
    <MenuItem x:Name="SaveMenuItem" Click="SaveMenuItem_Click" />
    <MenuItem x:Name="CloseMenuItem" Click="CloseMenuItem_Click" />
</MenuItem>
```

Для выбора языка используется `ComboBox`. При изменении выбранного языка вызывается обработчик, который обновляет все строки интерфейса.

## Локализация

Строки интерфейса вынесены в ресурсные файлы:

- `Resources/Strings.ru.resx` - русская локализация;
- `Resources/Strings.en.resx` - английская локализация.

В каждом файле определены одинаковые ключи: заголовок окна, пункты меню, подпись выбора языка, сообщения состояния, тексты ошибок и подтверждений.

Фрагмент `.resx`:

```xml
<data name="WindowTitle" xml:space="preserve">
  <value>Многоязычный блокнот</value>
</data>
```

Для загрузки строк создан класс `LocalizationManager`. Он ищет файлы вида `Strings.*.resx`, считывает их XML-структуру и сохраняет строки в словаре.

Фрагмент загрузки ресурсов:

```csharp
foreach (string filePath in Directory.GetFiles(resourcesDirectory, "Strings.*.resx"))
{
    string languageCode = Path.GetFileNameWithoutExtension(filePath)
        .Replace("Strings.", string.Empty, StringComparison.OrdinalIgnoreCase);
    _resources[languageCode] = ReadResx(filePath);
}
```

## Динамическое изменение языка

При выборе языка вызывается метод `SetLanguage`, после чего метод `ApplyLocalization` заново назначает заголовок окна, пункты меню, подпись языка и текст строки состояния.

Фрагмент переключения языка:

```csharp
private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
{
    if (LanguageComboBox.SelectedItem is LanguageOption language)
    {
        _localization.SetLanguage(language.Code);
        ApplyLocalization();
        UpdateWindowState();
    }
}
```

## Функциональность блокнота

Команда создания нового документа очищает текстовое поле и сбрасывает путь текущего файла. Перед этим выполняется проверка несохраненных изменений.

Открытие файла выполняется через `OpenFileDialog`. Содержимое выбранного файла считывается методом `File.ReadAllText` и отображается в текстовом поле.

Сохранение выполняется через `SaveFileDialog`, где пользователь выбирает имя и путь файла. После сохранения флаг несохраненных изменений сбрасывается.

## Подтверждение закрытия

Если в документе есть несохраненные изменения, при создании нового документа, открытии другого файла или закрытии приложения появляется диалог подтверждения. Текст диалога также берется из ресурсных файлов.

Фрагмент проверки:

```csharp
private bool ConfirmUnsavedChanges()
{
    if (!_hasUnsavedChanges)
    {
        return true;
    }

    MessageBoxResult result = MessageBox.Show(
        _localization.Get("UnsavedChangesMessage"),
        _localization.Get("UnsavedChangesTitle"),
        MessageBoxButton.YesNo,
        MessageBoxImage.Warning);

    return result == MessageBoxResult.Yes;
}
```

## Обработка ошибок

Операции открытия и сохранения файла помещены в блоки `try-catch`. При ошибке пользователь получает локализованное сообщение через `MessageBox`, а строка состояния показывает описание проблемы.

## Добавление новых языков

Чтобы добавить новый язык, нужно создать файл `Resources/Strings.<код>.resx` по аналогии с существующими файлами и заполнить в нем те же ключи. После сборки приложение автоматически найдет новый файл и добавит язык в список.

## Вывод

В результате выполнения лабораторной работы было создано многоязычное WPF-приложение. Были изучены ресурсные файлы `.resx`, динамическое переключение языка интерфейса, работа с текстовыми файлами, обработка ошибок и подтверждение закрытия при несохраненных изменениях.
