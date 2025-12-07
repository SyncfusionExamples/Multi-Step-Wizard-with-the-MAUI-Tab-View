using System.Text.RegularExpressions;

namespace MultiStepForm
{
    public partial class MainPage : ContentPage
    {
        private RegistrationWizard? _wizard;
        public MainPage()
        {
            InitializeComponent();
            // Hand off all wiring/logic to a separate class
            _wizard = new RegistrationWizard(this);
        }
    }
}
