using System.Text.RegularExpressions;

namespace MultiStepForm
{
    public partial class MainPage : ContentPage
    {
        private RegistrationWizard? _wizard;
        public MainPage()
        {
            InitializeComponent();
            // Move all wiring and logic into a separate class
            _wizard = new RegistrationWizard(this);
        }
    }
}
