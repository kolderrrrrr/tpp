using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace AnimatedCalculatorWPF
{
    public partial class MainWindow : Window
    {
        private double? _storedValue;
        private string? _pendingOperation;
        private bool _isNewInput = true;
        private bool _hasError;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CalculatorButton_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                button.RenderTransform = new ScaleTransform(1, 1);
            }
        }

        private void NumberButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Content is string digit)
            {
                AppendDigit(digit);
            }
        }

        private void DecimalButton_Click(object sender, RoutedEventArgs e)
        {
            ResetAfterError();

            if (_isNewInput)
            {
                SetDisplay("0.");
                _isNewInput = false;
                return;
            }

            if (!DisplayTextBlock.Text.Contains('.', StringComparison.Ordinal))
            {
                SetDisplay(DisplayTextBlock.Text + ".");
            }
        }

        private void OperationButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Content is not string operation || !TryGetDisplayValue(out double currentValue))
            {
                return;
            }

            if (_storedValue.HasValue && !_isNewInput && _pendingOperation is not null)
            {
                if (!TryCalculate(_storedValue.Value, currentValue, _pendingOperation, out double result))
                {
                    ShowError("Деление на ноль");
                    return;
                }

                _storedValue = result;
                AnimateDisplay(FormatNumber(result));
            }
            else
            {
                _storedValue = currentValue;
            }

            _pendingOperation = operation;
            _isNewInput = true;
            ExpressionTextBlock.Text = $"{FormatNumber(_storedValue.Value)} {operation}";
        }

        private void EqualsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_pendingOperation is null || !_storedValue.HasValue || !TryGetDisplayValue(out double currentValue))
            {
                return;
            }

            if (!TryCalculate(_storedValue.Value, currentValue, _pendingOperation, out double result))
            {
                ShowError("Деление на ноль");
                return;
            }

            ExpressionTextBlock.Text = $"{FormatNumber(_storedValue.Value)} {_pendingOperation} {FormatNumber(currentValue)} =";
            _storedValue = null;
            _pendingOperation = null;
            _isNewInput = true;
            AnimateDisplay(FormatNumber(result));
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _storedValue = null;
            _pendingOperation = null;
            _isNewInput = true;
            _hasError = false;
            ExpressionTextBlock.Text = string.Empty;
            AnimateClear();
        }

        private void Button_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Button button)
            {
                button.Background = new SolidColorBrush(Color.FromRgb(199, 220, 245));
            }
        }

        private void Button_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Button button)
            {
                button.ClearValue(Control.BackgroundProperty);
            }
        }

        private void AppendDigit(string digit)
        {
            ResetAfterError();

            if (_isNewInput || DisplayTextBlock.Text == "0")
            {
                SetDisplay(digit);
                _isNewInput = false;
                return;
            }

            if (DisplayTextBlock.Text.Length < 16)
            {
                SetDisplay(DisplayTextBlock.Text + digit);
            }
        }

        private void ResetAfterError()
        {
            if (!_hasError)
            {
                return;
            }

            _storedValue = null;
            _pendingOperation = null;
            _hasError = false;
            _isNewInput = true;
            ExpressionTextBlock.Text = string.Empty;
            SetDisplay("0");
        }

        private bool TryGetDisplayValue(out double value)
        {
            return double.TryParse(DisplayTextBlock.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private bool TryCalculate(double left, double right, string operation, out double result)
        {
            result = operation switch
            {
                "+" => left + right,
                "-" => left - right,
                "*" => left * right,
                "/" when Math.Abs(right) > double.Epsilon => left / right,
                _ => double.NaN
            };

            return !double.IsNaN(result) && !double.IsInfinity(result);
        }

        private void SetDisplay(string text)
        {
            DisplayTextBlock.Text = text;
        }

        private void AnimateDisplay(string text)
        {
            SetDisplay(text);

            Storyboard storyboard = new Storyboard();
            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                From = 0.2,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(220)
            };

            Storyboard.SetTarget(fadeAnimation, DisplayTextBlock);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(fadeAnimation);
            storyboard.Begin();
        }

        private void AnimateClear()
        {
            Storyboard fadeOut = new Storyboard();
            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(140),
                AutoReverse = true
            };

            fadeAnimation.Completed += (_, _) =>
            {
                DisplayTextBlock.Opacity = 1;
                SetDisplay("0");
            };

            Storyboard.SetTarget(fadeAnimation, DisplayTextBlock);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(UIElement.OpacityProperty));
            fadeOut.Children.Add(fadeAnimation);
            fadeOut.Begin();
        }

        private void ShowError(string message)
        {
            _storedValue = null;
            _pendingOperation = null;
            _isNewInput = true;
            _hasError = true;
            ExpressionTextBlock.Text = string.Empty;
            AnimateDisplay(message);
        }

        private string FormatNumber(double value)
        {
            return value.ToString("G12", CultureInfo.InvariantCulture);
        }
    }
}
