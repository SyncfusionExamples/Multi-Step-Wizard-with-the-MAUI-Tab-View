using System.Text.RegularExpressions;

namespace MultiStepForm;

public partial class PersonalInfoView : ContentView
{
    public event EventHandler? NextRequested;
    public PersonalInfoView()
	{
		InitializeComponent();

        BtnPersonalNext.Clicked += OnPersonalNextClicked;
    }

    // Validation helpers (kept local to this tab)
    static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        email = email.Trim().ToLowerInvariant();
        var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
    }

    static bool IsValidPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return false;
        var digits = Regex.Replace(phone, @"\D", "");
        return digits.Length >= 7 && digits.Length <= 10;
    }

    void SetError(Label label, string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            label.IsVisible = false;
            label.Text = "";
        }
        else
        {
            label.Text = message;
            label.IsVisible = true;
        }
    }

    async void OnPersonalNextClicked(object? sender, EventArgs e)
    {
        bool allEmpty =
            string.IsNullOrWhiteSpace(FullNameEntry.Text) &&
            string.IsNullOrWhiteSpace(EmailEntry.Text) &&
            string.IsNullOrWhiteSpace(MobileEntry.Text) &&
            string.IsNullOrWhiteSpace(OrgEntry.Text) &&
            string.IsNullOrWhiteSpace(JobTitleEntry.Text);

        if (allEmpty)
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is not null)
            {
                await page.DisplayAlertAsync("Missing Information", "Please fill the form to proceed.", "OK");
            }
            return;
        }

        SetError(FullNameError, string.IsNullOrWhiteSpace(FullNameEntry.Text) ? "Full name is required." : null);
        SetError(EmailError, !IsValidEmail(EmailEntry.Text) ? "Enter a valid email address." : null);
        SetError(MobileError, !IsValidPhone(MobileEntry.Text) ? "Enter a valid phone number." : null);
        SetError(OrgError, string.IsNullOrWhiteSpace(OrgEntry.Text) ? "Organization is required." : null);
        SetError(JobTitleError, string.IsNullOrWhiteSpace(JobTitleEntry.Text) ? "Job title is required." : null);

        bool anyError = FullNameError.IsVisible || EmailError.IsVisible || MobileError.IsVisible ||
                        OrgError.IsVisible || JobTitleError.IsVisible;
        if (anyError) return;

        NextRequested?.Invoke(this, EventArgs.Empty);
    }

    public void Reset()
    {
        FullNameEntry.Text = EmailEntry.Text = MobileEntry.Text = OrgEntry.Text = JobTitleEntry.Text = string.Empty;
        foreach (var lbl in new[] { FullNameError, EmailError, MobileError, OrgError, JobTitleError })
            SetError(lbl, null);
    }
}