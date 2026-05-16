using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

namespace LocalizedNotepadWPF
{
    public partial class MainWindow : Window
    {
        private readonly LocalizationManager _localization = new LocalizationManager();
        private string? _currentFilePath;
        private bool _hasUnsavedChanges;
        private bool _isLoadingDocument;
        private string _untitledFileName = "Untitled";
        private string _currentStatusKey = "StatusReady";

        public MainWindow()
        {
            InitializeComponent();
            RegisterShortcuts();
            LoadLanguages();
            ApplyLocalization();
            UpdateWindowState();
        }

        private void RegisterShortcuts()
        {
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => NewDocument()), Key.N, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => OpenDocument()), Key.O, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(new RelayCommand(_ => SaveDocumentAs()), Key.S, ModifierKeys.Control));
        }

        private void LoadLanguages()
        {
            List<LanguageOption> languages = _localization.GetLanguages().ToList();
            LanguageComboBox.ItemsSource = languages;

            LanguageOption? russian = languages.FirstOrDefault(language => language.Code == "ru");
            LanguageComboBox.SelectedItem = russian ?? languages.FirstOrDefault();
        }

        private void NewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            NewDocument();
        }

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenDocument();
        }

        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveDocumentAs();
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is not LanguageOption language)
            {
                return;
            }

            _localization.SetLanguage(language.Code);
            ApplyLocalization();
            UpdateWindowState();
        }

        private void EditorTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isLoadingDocument)
            {
                return;
            }

            _hasUnsavedChanges = true;
            UpdateWindowState();
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !ConfirmUnsavedChanges();
        }

        private void NewDocument()
        {
            if (!ConfirmUnsavedChanges())
            {
                return;
            }

            _isLoadingDocument = true;
            EditorTextBox.Clear();
            _isLoadingDocument = false;
            _currentFilePath = null;
            _hasUnsavedChanges = false;
            SetStatus("StatusNewDocument");
            UpdateWindowState();
        }

        private void OpenDocument()
        {
            if (!ConfirmUnsavedChanges())
            {
                return;
            }

            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = _localization.Get("TextFileFilter"),
                Title = _localization.Get("OpenDialogTitle")
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                _isLoadingDocument = true;
                EditorTextBox.Text = File.ReadAllText(dialog.FileName, Encoding.UTF8);
                _currentFilePath = dialog.FileName;
                _hasUnsavedChanges = false;
                SetStatus("StatusFileOpened");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                ShowError("OpenError", ex);
            }
            finally
            {
                _isLoadingDocument = false;
                UpdateWindowState();
            }
        }

        private void SaveDocumentAs()
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = _localization.Get("TextFileFilter"),
                FileName = GetSuggestedFileName(),
                Title = _localization.Get("SaveDialogTitle")
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                File.WriteAllText(dialog.FileName, EditorTextBox.Text, Encoding.UTF8);
                _currentFilePath = dialog.FileName;
                _hasUnsavedChanges = false;
                SetStatus("StatusFileSaved");
                UpdateWindowState();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                ShowError("SaveError", ex);
            }
        }

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

        private void ApplyLocalization()
        {
            _untitledFileName = _localization.Get("UntitledFileName");
            FileMenuItem.Header = _localization.Get("FileMenu");
            NewMenuItem.Header = _localization.Get("NewMenuItem");
            OpenMenuItem.Header = _localization.Get("OpenMenuItem");
            SaveMenuItem.Header = _localization.Get("SaveMenuItem");
            CloseMenuItem.Header = _localization.Get("CloseMenuItem");
            LanguageLabelTextBlock.Text = _localization.Get("LanguageLabel");
            StatusTextBlock.Text = _localization.Get(_currentStatusKey);
        }

        private void SetStatus(string statusKey)
        {
            _currentStatusKey = statusKey;
            StatusTextBlock.Text = _localization.Get(statusKey);
        }

        private string GetSuggestedFileName()
        {
            return string.IsNullOrWhiteSpace(_currentFilePath)
                ? _localization.Get("DefaultFileName")
                : Path.GetFileName(_currentFilePath);
        }

        private void UpdateWindowState()
        {
            string fileName = string.IsNullOrWhiteSpace(_currentFilePath)
                ? _untitledFileName
                : Path.GetFileName(_currentFilePath);

            string unsavedMarker = _hasUnsavedChanges ? "*" : string.Empty;
            Title = $"{_localization.Get("WindowTitle")} - {fileName}{unsavedMarker}";
            FileNameTextBlock.Text = $"{fileName}{unsavedMarker}";
        }

        private void ShowError(string messageKey, Exception exception)
        {
            string message = string.Format(CultureInfo.CurrentCulture, _localization.Get(messageKey), exception.Message);
            StatusTextBlock.Text = message;
            MessageBox.Show(
                message,
                _localization.Get("ErrorTitle"),
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    public sealed class LocalizationManager
    {
        private readonly Dictionary<string, Dictionary<string, string>> _resources = new Dictionary<string, Dictionary<string, string>>();
        private string _currentLanguage = "ru";

        public LocalizationManager()
        {
            LoadResourceFiles();
        }

        public IEnumerable<LanguageOption> GetLanguages()
        {
            return _resources
                .Select(pair => new LanguageOption(pair.Key, Get(pair.Key, "LanguageName")))
                .OrderBy(language => language.Name);
        }

        public void SetLanguage(string languageCode)
        {
            if (_resources.ContainsKey(languageCode))
            {
                _currentLanguage = languageCode;
            }
        }

        public string Get(string key)
        {
            return Get(_currentLanguage, key);
        }

        private string Get(string languageCode, string key)
        {
            if (_resources.TryGetValue(languageCode, out Dictionary<string, string>? languageResources) &&
                languageResources.TryGetValue(key, out string? value))
            {
                return value;
            }

            if (_resources.TryGetValue("en", out Dictionary<string, string>? englishResources) &&
                englishResources.TryGetValue(key, out string? fallbackValue))
            {
                return fallbackValue;
            }

            return key;
        }

        private void LoadResourceFiles()
        {
            string resourcesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");

            if (!Directory.Exists(resourcesDirectory))
            {
                resourcesDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "Resources");
            }

            if (!Directory.Exists(resourcesDirectory))
            {
                return;
            }

            foreach (string filePath in Directory.GetFiles(resourcesDirectory, "Strings.*.resx"))
            {
                string languageCode = Path.GetFileNameWithoutExtension(filePath).Replace("Strings.", string.Empty, StringComparison.OrdinalIgnoreCase);
                _resources[languageCode] = ReadResx(filePath);
            }
        }

        private static Dictionary<string, string> ReadResx(string filePath)
        {
            XDocument document = XDocument.Load(filePath);
            return document
                .Root?
                .Elements("data")
                .Where(element => element.Attribute("name") is not null)
                .ToDictionary(
                    element => element.Attribute("name")!.Value,
                    element => element.Element("value")?.Value ?? string.Empty)
                ?? new Dictionary<string, string>();
        }
    }

    public sealed class LanguageOption
    {
        public LanguageOption(string code, string name)
        {
            Code = code;
            Name = name;
        }

        public string Code { get; }

        public string Name { get; }
    }

    public sealed class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;

        public RelayCommand(Action<object?> execute)
        {
            _execute = execute;
        }

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}
