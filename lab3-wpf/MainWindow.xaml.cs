using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NotesWPF
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<string> _notes = new ObservableCollection<string>();
        private readonly string _storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "notes.txt");
        private bool _isSelectionChanging;

        public MainWindow()
        {
            InitializeComponent();
            LoadNotes();
            RefreshNotesList();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string note = NoteTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(note))
            {
                ShowStatus("Введите текст заметки.");
                return;
            }

            _notes.Add(note);
            NoteTextBox.Clear();
            RefreshNotesList();
            ShowStatus("Заметка добавлена.");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = GetSelectedNoteIndex();

            if (selectedIndex < 0)
            {
                ShowStatus("Выберите заметку для сохранения.");
                return;
            }

            string note = NoteTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(note))
            {
                ShowStatus("Текст заметки не должен быть пустым.");
                return;
            }

            _notes[selectedIndex] = note;
            RefreshNotesList(note);
            ShowStatus("Заметка сохранена.");
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            int selectedIndex = GetSelectedNoteIndex();

            if (selectedIndex < 0)
            {
                ShowStatus("Выберите заметку для удаления.");
                return;
            }

            _notes.RemoveAt(selectedIndex);
            NoteTextBox.Clear();
            RefreshNotesList();
            ShowStatus("Заметка удалена.");
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            NotesListBox.SelectedIndex = -1;
            NoteTextBox.Clear();
            ShowStatus("Поле ввода очищено.");
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshNotesList();
        }

        private void NotesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isSelectionChanging || NotesListBox.SelectedItem is not NoteListItem item)
            {
                return;
            }

            NoteTextBox.Text = _notes[item.Index];
            ShowStatus("Заметка загружена для редактирования.");
        }

        private void ColorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem menuItem || menuItem.Tag is not string color)
            {
                return;
            }

            if (new BrushConverter().ConvertFromString(color) is Brush brush)
            {
                NoteTextBox.Background = brush;
            }

            ShowStatus("Цвет фона изменен.");
        }

        private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveNotesToFile();
        }

        private void LoadNotes()
        {
            if (!File.Exists(_storagePath))
            {
                return;
            }

            string fileContent = File.ReadAllText(_storagePath);

            if (string.IsNullOrWhiteSpace(fileContent))
            {
                return;
            }

            List<string>? savedNotes = JsonSerializer.Deserialize<List<string>>(fileContent);

            foreach (string note in savedNotes?.Where(line => !string.IsNullOrWhiteSpace(line)) ?? Enumerable.Empty<string>())
            {
                _notes.Add(note);
            }
        }

        private void SaveNotesToFile()
        {
            string fileContent = JsonSerializer.Serialize(_notes, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_storagePath, fileContent);
        }

        private void RefreshNotesList(string? noteToSelect = null)
        {
            string searchText = SearchTextBox?.Text.Trim() ?? string.Empty;
            IEnumerable<NoteListItem> filteredNotes = _notes
                .Select((note, index) => new NoteListItem(index, note))
                .Where(item => string.IsNullOrEmpty(searchText) ||
                               item.Text.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);

            _isSelectionChanging = true;
            NotesListBox.ItemsSource = filteredNotes.ToList();
            NotesListBox.DisplayMemberPath = nameof(NoteListItem.Text);

            if (!string.IsNullOrEmpty(noteToSelect))
            {
                NotesListBox.SelectedItem = NotesListBox.Items
                    .OfType<NoteListItem>()
                    .FirstOrDefault(item => item.Text == noteToSelect);
            }

            _isSelectionChanging = false;
        }

        private int GetSelectedNoteIndex()
        {
            return NotesListBox.SelectedItem is NoteListItem item ? item.Index : -1;
        }

        private void ShowStatus(string message)
        {
            StatusTextBlock.Text = message;
        }

        private sealed class NoteListItem
        {
            public NoteListItem(int index, string text)
            {
                Index = index;
                Text = text;
            }

            public int Index { get; }

            public string Text { get; }
        }
    }
}
