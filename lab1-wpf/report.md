# Отчет по лабораторной работе 1

## Тема

Создание простого графического приложения с использованием Windows Presentation Foundation.

## Цель работы

Научиться создавать простое графическое приложение WPF, которое отображает сообщение `Hello, World!` на экране.

## Использованные технологии

В ходе работы использовались:

- среда разработки Microsoft Visual Studio;
- платформа .NET;
- технология Windows Presentation Foundation;
- язык разметки XAML.

WPF применяется для разработки настольных приложений Windows с графическим интерфейсом. Интерфейс приложения описывается с помощью XAML, а логика программы может быть реализована на C#.

## Ход выполнения работы

Сначала был создан новый проект WPF-приложения в Visual Studio. После создания проекта был открыт файл `MainWindow.xaml`, который отвечает за описание главного окна приложения.

Внутри элемента `Grid` был размещен элемент `TextBlock`. Для него был задан текст `Hello, World!`, выравнивание по центру окна, размер шрифта и цвет текста. Также были настроены заголовок окна, размеры окна и цвет фона.

Фрагмент разметки главного окна:

```xml
<Window x:Class="HelloWorldWPF.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Hello World" Height="200" Width="400"
        Background="#F4F8FF"
        WindowStartupLocation="CenterScreen">
    <Grid>
        <TextBlock Text="Hello, World!"
                   HorizontalAlignment="Center"
                   VerticalAlignment="Center"
                   FontSize="24"
                   FontWeight="SemiBold"
                   Foreground="Blue" />
    </Grid>
</Window>
```

После запуска приложения на экране появилось окно с сообщением `Hello, World!`, расположенным по центру.

## Возникшие трудности

Основная сложность заключалась в понимании структуры XAML-файла и назначения основных свойств элементов интерфейса. После изучения свойств `HorizontalAlignment`, `VerticalAlignment`, `FontSize` и `Foreground` стало понятно, как управлять внешним видом текстового элемента.

## Вывод

В результате выполнения лабораторной работы было создано простое WPF-приложение. Были изучены основы создания проекта в Visual Studio, структура файла `MainWindow.xaml`, базовые элементы управления WPF и способы простой стилизации интерфейса.
