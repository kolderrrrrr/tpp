using Microsoft.Win32;
using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace NotepadWPF
{
    public partial class MainWindow : Window
    {
        private string? _currentFilePath;
        private bool _hasUnsavedChanges;
        private bool _isLoadingDocument;

        public MainWindow()
        {
            InitializeComponent();
            UpdateWindowState();
        }

        private void Command_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            EditorTextBox.Clear();
            _currentFilePath = null;
            _hasUnsavedChanges = false;
            UpdateWindowState("Создан новый документ.");
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                Title = "Открыть файл"
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
                UpdateWindowState("Файл открыт.");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                ShowError("Не удалось открыть файл.", ex);
            }
            finally
            {
                _isLoadingDocument = false;
            }
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_currentFilePath))
            {
                SaveDocumentAs();
                return;
            }

            SaveDocument(_currentFilePath);
        }

        private void SaveAsCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveDocumentAs();
        }

        private void EditorTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (_isLoadingDocument)
            {
                return;
            }

            _hasUnsavedChanges = true;
            UpdateWindowState();
        }

        private void SaveDocumentAs()
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                FileName = GetSuggestedFileName(),
                Title = "Сохранить файл"
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            SaveDocument(dialog.FileName);
        }

        private void SaveDocument(string filePath)
        {
            try
            {
                File.WriteAllText(filePath, EditorTextBox.Text, Encoding.UTF8);
                _currentFilePath = filePath;
                _hasUnsavedChanges = false;
                UpdateWindowState("Файл сохранен.");
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
            {
                ShowError("Не удалось сохранить файл.", ex);
            }
        }

        private string GetSuggestedFileName()
        {
            return string.IsNullOrWhiteSpace(_currentFilePath)
                ? "Новый документ.txt"
                : Path.GetFileName(_currentFilePath);
        }

        private void UpdateWindowState(string? status = null)
        {
            string fileName = string.IsNullOrWhiteSpace(_currentFilePath)
                ? "Новый документ"
                : Path.GetFileName(_currentFilePath);

            string unsavedMarker = _hasUnsavedChanges ? "*" : string.Empty;
            Title = $"Блокнот - {fileName}{unsavedMarker}";
            FileNameTextBlock.Text = $"{fileName}{unsavedMarker}";

            if (!string.IsNullOrWhiteSpace(status))
            {
                StatusTextBlock.Text = status;
            }
        }

        private void ShowError(string message, Exception exception)
        {
            StatusTextBlock.Text = message;
            MessageBox.Show(
                $"{message}\n\n{exception.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
}
