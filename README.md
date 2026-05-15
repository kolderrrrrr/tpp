# Лабораторные работы по ТПП

Репозиторий содержит три лабораторные работы на WPF:

- `lab1-wpf` - приложение `Hello, World!`;
- `lab2-wpf` - калькулятор с базовыми арифметическими операциями;
- `lab3-wpf` - менеджер текстовых заметок.

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
   - `NotesWPF` для лабораторной 3.
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
└── lab3-wpf
    ├── NotesWPF.csproj
    ├── App.xaml
    ├── App.xaml.cs
    ├── MainWindow.xaml
    ├── MainWindow.xaml.cs
    ├── README.md
    └── report.md
```

## Примечание

WPF-приложения запускаются на Windows. На macOS можно редактировать файлы и загружать их на GitHub, но проверять запуск лучше в Visual Studio на Windows.
