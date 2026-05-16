using System;
using System.Windows;
using System.Windows.Controls;

namespace MultiStepFormWPF
{
    public partial class PersonalPage : Page
    {
        private readonly FormData _formData;

        public PersonalPage(FormData formData)
        {
            InitializeComponent();
            _formData = formData;
            LoadData();
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate())
            {
                return;
            }

            SaveData();
            NavigationService?.Navigate(new ContactPage(_formData));
        }

        private void LoadData()
        {
            FirstNameTextBox.Text = _formData.FirstName;
            LastNameTextBox.Text = _formData.LastName;
            BirthDatePicker.SelectedDate = _formData.BirthDate;
        }

        private void SaveData()
        {
            _formData.FirstName = FirstNameTextBox.Text.Trim();
            _formData.LastName = LastNameTextBox.Text.Trim();
            _formData.BirthDate = BirthDatePicker.SelectedDate;
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(FirstNameTextBox.Text))
            {
                ErrorTextBlock.Text = "Введите имя.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(LastNameTextBox.Text))
            {
                ErrorTextBlock.Text = "Введите фамилию.";
                return false;
            }

            if (BirthDatePicker.SelectedDate is null)
            {
                ErrorTextBlock.Text = "Выберите дату рождения.";
                return false;
            }

            if (BirthDatePicker.SelectedDate.Value > DateTime.Today)
            {
                ErrorTextBlock.Text = "Дата рождения не может быть в будущем.";
                return false;
            }

            ErrorTextBlock.Text = string.Empty;
            return true;
        }
    }
}
