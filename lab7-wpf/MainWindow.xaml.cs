using System;
using System.Windows;
using System.Windows.Controls;

namespace CustomButtonTemplateWPF
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Content is not string actionName)
            {
                return;
            }

            string message = $"{DateTime.Now:HH:mm:ss} - нажата кнопка «{actionName}»";
            ActionListBox.Items.Insert(0, message);
            StatusTextBlock.Text = message;
        }
    }
}
