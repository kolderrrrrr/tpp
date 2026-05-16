using System.Text.RegularExpressions;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MultiStepFormWPF
{
    public partial class ContactPage : Page
    {
        private readonly FormData _formData;
        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);
        private static readonly Regex PhoneRegex = new Regex(@"^\+?[0-9\s\-\(\)]{7,20}$", RegexOptions.Compiled);

        public ContactPage(FormData formData)
        {
            InitializeComponent();
            _formData = formData;
            LoadData();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            SaveData();
            NavigationService?.Navigate(new PersonalPage(_formData));
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Validate())
            {
                return;
            }

            SaveData();
            NavigationService?.Navigate(new AddressPage(_formData));
        }

        private void LoadData()
        {
            EmailTextBox.Text = _formData.Email;
            PhoneTextBox.Text = _formData.Phone;
        }

        private void SaveData()
        {
            _formData.Email = EmailTextBox.Text.Trim();
            _formData.Phone = PhoneTextBox.Text.Trim();
        }

        private bool Validate()
        {
            string email = EmailTextBox.Text.Trim();
            string phone = PhoneTextBox.Text.Trim();

            if (!EmailRegex.IsMatch(email))
            {
                ErrorTextBlock.Text = "Введите корректный email.";
                return false;
            }

            if (!PhoneRegex.IsMatch(phone) || phone.Count(char.IsDigit) < 7)
            {
                ErrorTextBlock.Text = "Введите корректный номер телефона.";
                return false;
            }

            ErrorTextBlock.Text = string.Empty;
            return true;
        }
    }
}
