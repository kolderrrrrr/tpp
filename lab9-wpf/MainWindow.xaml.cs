using System.Windows;

namespace MultiStepFormWPF
{
    public partial class MainWindow : Window
    {
        private readonly FormData _formData = new FormData();

        public MainWindow()
        {
            InitializeComponent();
            FormFrame.Navigate(new PersonalPage(_formData));
        }
    }
}
