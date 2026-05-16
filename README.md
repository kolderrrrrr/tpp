# Лабораторные работы по ТПП

Репозиторий содержит девять лабораторных работ на WPF:

- `lab1-wpf` - приложение `Hello, World!`;
- `lab2-wpf` - калькулятор с базовыми арифметическими операциями;
- `lab3-wpf` - менеджер текстовых заметок;
- `lab4-wpf` - игра `Угадай число` с привязкой данных;
- `lab5-wpf` - текстовый редактор `Блокнот` с командами;
- `lab6-wpf` - анимированный калькулятор;
- `lab7-wpf` - пользовательский шаблон кнопки;
- `lab8-wpf` - многоязычный блокнот;
- `lab9-wpf` - многоступенчатая форма с навигацией.

## Как запустить на Windows

1. Установить Visual Studio 2022.
2. При установке выбрать компонент `Разработка классических приложений .NET`.
3. Склонировать репозиторий:

```bash
git clone <ссылка-на-репозиторий>
```

4. Открыть файл `TppLabs.sln` в Visual Studio.
5. В `Solution Explorer` выбрать нужный проект:
   - `HelloWorldWPF` для лабораторной 1;
   - `CalculatorWPF` для лабораторной 2;
   - `NotesWPF` для лабораторной 3;
   - `GuessNumberWPF` для лабораторной 4;
   - `NotepadWPF` для лабораторной 5;
   - `AnimatedCalculatorWPF` для лабораторной 6;
   - `CustomButtonTemplateWPF` для лабораторной 7;
   - `LocalizedNotepadWPF` для лабораторной 8;
   - `MultiStepFormWPF` для лабораторной 9.
6. Нажать `F5` для запуска.

## Структура

```text
.
├── TppLabs.sln
├── lab1-wpf
│   ├── HelloWorldWPF.csproj
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── README.md
│   └── report.md
├── lab2-wpf
│   ├── CalculatorWPF.csproj
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── README.md
│   └── report.md
├── lab3-wpf
│   ├── NotesWPF.csproj
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── README.md
│   └── report.md
├── lab4-wpf
│   ├── GuessNumberWPF.csproj
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── README.md
│   └── report.md
├── lab5-wpf
│   ├── NotepadWPF.csproj
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── README.md
│   └── report.md
├── lab6-wpf
│   ├── AnimatedCalculatorWPF.csproj
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── README.md
│   └── report.md
├── lab7-wpf
│   ├── CustomButtonTemplateWPF.csproj
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── README.md
│   └── report.md
├── lab8-wpf
│   ├── LocalizedNotepadWPF.csproj
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── Resources
│   │   ├── Strings.en.resx
│   │   └── Strings.ru.resx
│   ├── README.md
│   └── report.md
└── lab9-wpf
    ├── MultiStepFormWPF.csproj
    ├── App.xaml
    ├── App.xaml.cs
    ├── FormData.cs
    ├── MainWindow.xaml
    ├── MainWindow.xaml.cs
    ├── PersonalPage.xaml
    ├── PersonalPage.xaml.cs
    ├── ContactPage.xaml
    ├── ContactPage.xaml.cs
    ├── AddressPage.xaml
    ├── AddressPage.xaml.cs
    ├── README.md
    └── report.md
```

## Примечание

WPF-приложения запускаются на Windows. На macOS можно редактировать файлы и загружать их на GitHub, но проверять запуск лучше в Visual Studio на Windows.
