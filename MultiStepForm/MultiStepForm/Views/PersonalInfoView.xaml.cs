using Syncfusion.Maui.Core;                 
using System.Text.RegularExpressions;       

namespace MultiStepForm                      
{
    public partial class PersonalInfoView : ContentView   
    {
        public event EventHandler? NextRequested;         // Event raised when the "Next" action is valid

        private static readonly Regex PhoneRegex = new Regex(@"^\+?[0-9\s\-\(\)]{7,12}$", RegexOptions.Compiled);       // Compiled regex for phone number validation

        private static readonly Regex EmailRegex = new Regex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);      // Compiled, case-insensitive regex for basic email validation

        private SfTextInputLayout[]? _allFields;          // References to all input layouts

        public PersonalInfoView()                         
        {
            InitializeComponent();                        

            BtnPersonalNext.Clicked += OnPersonalNextClicked;  // Subscribes to the Next button click event

            _allFields = new[]                            // Initializes the array with all SfTextInputLayout fields
            {
                NameField, EmailField, PhoneNumberField, OrganizationField, JobTitleField
            };
        }

        private void ValidateAll()                       // Validates all fields at once
        {
            foreach (var field in _allFields!)           // Iterates through all fields
            {
                FieldNullCheck(field);                   
            }
            ValidatePhoneNumber();                       // Phone-specific validation
            ValidateEmailAddress();                      // Email-specific validation
        }

        private static string GetTextTrimmed(SfTextInputLayout inputLayout) // Get trimmed text from a layout
            => inputLayout.Text?.Trim() ?? string.Empty; // Null-safe access; trims whitespace; returns empty if null

        private static void FieldNullCheck(SfTextInputLayout inputLayout)   // Checks if a field is empty or whitespace
        {
            inputLayout.HasError = string.IsNullOrWhiteSpace(GetTextTrimmed(inputLayout));
            // Sets HasError to true if the text is null/empty/whitespace
        }

        private void ValidatePhoneNumber()               // Validates the phone number format
        {
            var text = GetTextTrimmed(PhoneNumberField); // Read and trim the phone input
            if (!string.IsNullOrEmpty(text))             // Only validate when user entered something
            {
                PhoneNumberField.HasError = !PhoneRegex.IsMatch(text); // Error if it doesn't match the regex
            }
        }

        private void ValidateEmailAddress()              // Validates the email format
        {
            var text = GetTextTrimmed(EmailField);       // Read and trim the email input
            if (!string.IsNullOrEmpty(text))             // Only validate when user entered something
            {
                EmailField.HasError = !EmailRegex.IsMatch(text); // Error if regex doesn't match
            }
        }

        private void OnPersonalNextClicked(object? sender, EventArgs e) // Handler for the Next button click
        {
            ValidateAll();                                // Run all validations

            bool anyError = _allFields!.Any(fields => fields.HasError); // Check if any field has an error
            if (anyError) return;                         // Abort navigation if there are validation errors

            NextRequested?.Invoke(this, EventArgs.Empty); // Raise NextRequested if there are no errors
        }

        public void Reset()                               // Resets all inputs to empty
        {
            FullNameEntry.Text = EmailEntry.Text = MobileEntry.Text = OrgEntry.Text =
            JobTitleEntry.Text = string.Empty;            // Sets all texts to empty string
        }
    }
}