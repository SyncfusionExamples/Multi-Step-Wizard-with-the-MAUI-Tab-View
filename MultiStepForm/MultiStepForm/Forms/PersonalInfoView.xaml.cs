using System.Text.RegularExpressions;            

namespace MultiStepForm                             
{
    public partial class PersonalInfoView : ContentView 
    {
        public event EventHandler? NextRequested;       // Event raised when the user successfully completes this step

        public PersonalInfoView()                       
        {
            InitializeComponent();                      
            BtnPersonalNext.Clicked += OnPersonalNextClicked; // Subscribes the button click to the handler
        }

        static bool IsValidEmail(string? email)         // Validate email format
        {
            if (string.IsNullOrWhiteSpace(email)) return false;      // Reject null/empty/whitespace
            email = email.Trim().ToLowerInvariant();                 // Normalize input
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";             // Basic email regex
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase); // Test with case-insensitive match
        }

        static bool IsValidPhone(string? phone)         // Validate phone numbers
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;      // Reject null/empty
            var digits = Regex.Replace(phone, @"\D", "");            // Strip all non-digits
            return digits.Length >= 7 && digits.Length <= 12;        // Accept if digit count within range
        }

        void SetError(Label label, string? message)     // Shows/hides an error label with a message
        {
            if (string.IsNullOrWhiteSpace(message))     // No error -> hide label
            {
                label.IsVisible = false;
                label.Text = "";
            }
            else                                        // Error -> set text and show label
            {
                label.Text = message;
                label.IsVisible = true;
            }
        }

        async void OnPersonalNextClicked(object? sender, EventArgs e) // Click handler for Next button
        {
            // Check if all fields are empty, to prompt the user to fill the form
            bool allEmpty =
                string.IsNullOrWhiteSpace(FullNameEntry.Text) &&
                string.IsNullOrWhiteSpace(EmailEntry.Text) &&
                string.IsNullOrWhiteSpace(MobileEntry.Text) &&
                string.IsNullOrWhiteSpace(OrgEntry.Text) &&
                string.IsNullOrWhiteSpace(JobTitleEntry.Text);

            if (allEmpty)
            {
                var page = Application.Current?.Windows.FirstOrDefault()?.Page; // Get current top page 
                if (page is not null)
                {
                    // Show an alert prompting user to fill the form
                    await page.DisplayAlertAsync("Missing Information", "Please fill the form to proceed.", "OK");
                }
                return; 
            }

            // Validation with error messages
            SetError(FullNameError, string.IsNullOrWhiteSpace(FullNameEntry.Text) ? "Full name is required." : null);
            SetError(EmailError, !IsValidEmail(EmailEntry.Text) ? "Enter a valid email address." : null);
            SetError(MobileError, !IsValidPhone(MobileEntry.Text) ? "Enter a valid phone number." : null);
            SetError(OrgError, string.IsNullOrWhiteSpace(OrgEntry.Text) ? "Organization is required." : null);
            SetError(JobTitleError, string.IsNullOrWhiteSpace(JobTitleEntry.Text) ? "Job title is required." : null);

            // Determine if any error label is visible after validation
            bool anyError = FullNameError.IsVisible || EmailError.IsVisible || MobileError.IsVisible ||
                            OrgError.IsVisible || JobTitleError.IsVisible;

            if (anyError) return;             

            NextRequested?.Invoke(this, EventArgs.Empty); // Raise event to move to next step
        }

        public void Reset()                    // Clears the form and hides all error messages
        {
            // Clear text for all fields 
            FullNameEntry.Text = EmailEntry.Text = MobileEntry.Text = OrgEntry.Text = JobTitleEntry.Text = string.Empty;

            // Hide and clear all error labels
            foreach (var lbl in new[] { FullNameError, EmailError, MobileError, OrgError, JobTitleError })
                SetError(lbl, null);
        }
    }
}