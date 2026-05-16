using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace MultiStepFormWPF
{
    public partial class AddressPage : Page
    {
        private readonly FormData _formData;

        public AddressPage(FormData formData)
        {
            InitializeComponent();
            _formData = formData;
            LoadData();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
            NavigationService?.Navigate(new ContactPage(_formData));
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate())
            {
                return;
            }

            SaveData();
            MessageBox.Show(BuildSummary(), "Введенные данные", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadData()
        {
            CityTextBox.Text = _formData.City;
            StreetTextBox.Text = _formData.Street;
            HouseNumberTextBox.Text = _formData.HouseNumber;
            ApartmentNumberTextBox.Text = _formData.ApartmentNumber;
        }

        private void SaveData()
        {
            _formData.City = CityTextBox.Text.Trim();
            _formData.Street = StreetTextBox.Text.Trim();
            _formData.HouseNumber = HouseNumberTextBox.Text.Trim();
            _formData.ApartmentNumber = ApartmentNumberTextBox.Text.Trim();
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(CityTextBox.Text))
            {
                ErrorTextBlock.Text = "Введите город.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(StreetTextBox.Text))
            {
                ErrorTextBlock.Text = "Введите улицу.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(HouseNumberTextBox.Text))
            {
                ErrorTextBlock.Text = "Введите номер дома.";
                return false;
            }

            ErrorTextBlock.Text = string.Empty;
            return true;
        }

        private string BuildSummary()
        {
            string birthDate = _formData.BirthDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-";
            string apartment = string.IsNullOrWhiteSpace(_formData.ApartmentNumber) ? "-" : _formData.ApartmentNumber;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"Имя: {_formData.FirstName}");
            builder.AppendLine($"Фамилия: {_formData.LastName}");
            builder.AppendLine($"Дата рождения: {birthDate}");
            builder.AppendLine();
            builder.AppendLine($"Email: {_formData.Email}");
            builder.AppendLine($"Телефон: {_formData.Phone}");
            builder.AppendLine();
            builder.AppendLine($"Город: {_formData.City}");
            builder.AppendLine($"Улица: {_formData.Street}");
            builder.AppendLine($"Дом: {_formData.HouseNumber}");
            builder.AppendLine($"Квартира: {apartment}");
            return builder.ToString();
        }
    }
}
