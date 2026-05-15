using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace CalculatorWPF
{
    public partial class MainWindow : Window
    {
        private bool _isResultShown;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InputButton_Click(object sender, RoutedEventArgs e)
        {
            string value = ((Button)sender).Tag.ToString();

            if (_isResultShown && IsDigitOrDecimal(value))
            {
                DisplayTextBox.Text = "0";
            }

            _isResultShown = false;

            if (DisplayTextBox.Text == "0" && IsDigitOrDecimal(value) && value != ".")
            {
                DisplayTextBox.Text = value;
            }
            else
            {
                DisplayTextBox.Text += value;
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayTextBox.Text = "0";
            _isResultShown = false;
        }

        private void EqualsButton_Click(object sender, RoutedEventArgs e)
        {
            string expression = DisplayTextBox.Text;

            try
            {
                double result = new ExpressionParser(expression).Parse();
                string resultText = result.ToString("G10", CultureInfo.InvariantCulture);

                DisplayTextBox.Text = resultText;
                HistoryListBox.Items.Insert(0, expression + " = " + resultText);
                _isResultShown = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка вычисления", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private static bool IsDigitOrDecimal(string value)
        {
            return char.IsDigit(value[0]) || value == ".";
        }
    }

    internal class ExpressionParser
    {
        private readonly string _expression;
        private int _position;

        public ExpressionParser(string expression)
        {
            _expression = expression.Replace(" ", string.Empty);
        }

        public double Parse()
        {
            if (string.IsNullOrWhiteSpace(_expression))
            {
                throw new InvalidOperationException("Введите выражение.");
            }

            double result = ParseExpression();

            if (_position < _expression.Length)
            {
                throw new InvalidOperationException("Некорректный символ в выражении.");
            }

            return result;
        }

        private double ParseExpression()
        {
            double result = ParseTerm();

            while (Match('+') || Match('-'))
            {
                char operation = _expression[_position - 1];
                double nextValue = ParseTerm();
                result = operation == '+' ? result + nextValue : result - nextValue;
            }

            return result;
        }

        private double ParseTerm()
        {
            double result = ParseFactor();

            while (Match('*') || Match('/'))
            {
                char operation = _expression[_position - 1];
                double nextValue = ParseFactor();

                if (operation == '/')
                {
                    if (Math.Abs(nextValue) < 0.000000001)
                    {
                        throw new DivideByZeroException("Деление на ноль невозможно.");
                    }

                    result /= nextValue;
                }
                else
                {
                    result *= nextValue;
                }
            }

            return result;
        }

        private double ParseFactor()
        {
            if (Match('+'))
            {
                return ParseFactor();
            }

            if (Match('-'))
            {
                return -ParseFactor();
            }

            if (Match('('))
            {
                double result = ParseExpression();

                if (!Match(')'))
                {
                    throw new InvalidOperationException("Не закрыта скобка.");
                }

                return result;
            }

            return ParseNumber();
        }

        private double ParseNumber()
        {
            int start = _position;

            while (_position < _expression.Length &&
                   (char.IsDigit(_expression[_position]) || _expression[_position] == '.'))
            {
                _position++;
            }

            if (start == _position)
            {
                throw new InvalidOperationException("Ожидалось число.");
            }

            string number = _expression.Substring(start, _position - start);

            if (!double.TryParse(number, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                throw new InvalidOperationException("Некорректное число.");
            }

            return value;
        }

        private bool Match(char expected)
        {
            if (_position >= _expression.Length || _expression[_position] != expected)
            {
                return false;
            }

            _position++;
            return true;
        }
    }
}
