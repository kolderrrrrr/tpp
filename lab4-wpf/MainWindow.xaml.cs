using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace GuessNumberWPF
{
    public partial class MainWindow : Window
    {
        private readonly GameState _gameState = new GameState();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _gameState;
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            _gameState.CheckGuess();
            GuessTextBox.SelectAll();
            GuessTextBox.Focus();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            _gameState.Restart();
            GuessTextBox.Focus();
        }

        private void GuessTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }
    }

    public sealed class GameState : INotifyPropertyChanged
    {
        private const int MinNumber = 1;
        private const int MaxNumber = 100;
        private readonly Random _random = new Random();
        private string _currentGuess = string.Empty;
        private int _targetNumber;
        private int _attempts;
        private string _message = string.Empty;
        private string _messageBrush = NeutralBrush;
        private bool _isFinished;

        private const string NeutralBrush = "#4B5563";
        private const string ErrorBrush = "#D64545";
        private const string WarningBrush = "#B7791F";
        private const string SuccessBrush = "#1F8A4C";

        public GameState()
        {
            Restart();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Hint => $"Угадайте число от {MinNumber} до {MaxNumber}";

        public string CurrentGuess
        {
            get => _currentGuess;
            set
            {
                if (_currentGuess == value)
                {
                    return;
                }

                _currentGuess = value;
                OnPropertyChanged();
            }
        }

        public int Attempts
        {
            get => _attempts;
            private set
            {
                if (_attempts == value)
                {
                    return;
                }

                _attempts = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(AttemptsText));
                OnPropertyChanged(nameof(SearchProgress));
            }
        }

        public string AttemptsText => Attempts == 0
            ? "Попыток пока нет"
            : $"Количество попыток: {Attempts}";

        public double SearchProgress => Math.Min(Attempts * 10, 100);

        public string Message
        {
            get => _message;
            private set
            {
                if (_message == value)
                {
                    return;
                }

                _message = value;
                OnPropertyChanged();
            }
        }

        public string MessageBrush
        {
            get => _messageBrush;
            private set
            {
                if (_messageBrush == value)
                {
                    return;
                }

                _messageBrush = value;
                OnPropertyChanged();
            }
        }

        public void CheckGuess()
        {
            if (_isFinished)
            {
                SetMessage("Игра завершена. Нажмите «Новая игра», чтобы начать заново.", NeutralBrush);
                return;
            }

            if (!int.TryParse(CurrentGuess.Trim(), out int guess))
            {
                SetMessage("Введите целое число.", ErrorBrush);
                return;
            }

            if (guess < MinNumber || guess > MaxNumber)
            {
                SetMessage($"Число должно быть в диапазоне от {MinNumber} до {MaxNumber}.", ErrorBrush);
                return;
            }

            Attempts++;

            if (guess < _targetNumber)
            {
                SetMessage("Слишком маленькое число.", WarningBrush);
                return;
            }

            if (guess > _targetNumber)
            {
                SetMessage("Слишком большое число.", WarningBrush);
                return;
            }

            _isFinished = true;
            SetMessage($"Поздравляем, вы угадали число за {Attempts} попыток!", SuccessBrush);
        }

        public void Restart()
        {
            _targetNumber = _random.Next(MinNumber, MaxNumber + 1);
            _isFinished = false;
            CurrentGuess = string.Empty;
            Attempts = 0;
            SetMessage("Новая игра началась.", NeutralBrush);
            OnPropertyChanged(nameof(Hint));
        }

        private void SetMessage(string message, string brush)
        {
            Message = message;
            MessageBrush = brush;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
