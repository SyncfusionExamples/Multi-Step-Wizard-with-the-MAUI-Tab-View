namespace MultiStepForm;

public partial class EventSelectionView : ContentView
{
    public event EventHandler? BackRequested;
    public event EventHandler? NextRequested;
    public EventSelectionView()
	{
		InitializeComponent();

        BtnBackToPersonal.Clicked += (_, __) => BackRequested?.Invoke(this, EventArgs.Empty);
        BtnEventNext.Clicked += OnEventNextClicked;
    }

    void SetError(Label label, string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            label.IsVisible = false; label.Text = "";
        }
        else
        {
            label.Text = message; label.IsVisible = true;
        }
    }

    async void OnEventNextClicked(object? sender, EventArgs e)
    {
        bool allEmpty =
            string.IsNullOrWhiteSpace(EventNameEntry.Text) &&
            !CbKeynotes.IsChecked && !CbWorkshops.IsChecked && !CbBreakouts.IsChecked &&
            TrackPicker.SelectedIndex < 0;

        if (allEmpty)
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is not null)
            {
                await page.DisplayAlertAsync("Missing Information", "Please choose your event selections.", "OK");
            }
            return;
        }

        bool anySession = CbKeynotes.IsChecked || CbWorkshops.IsChecked || CbBreakouts.IsChecked;
        SetError(EventNameError, string.IsNullOrWhiteSpace(EventNameEntry.Text) ? "Event / conference name is required." : null);
        SetError(SessionError, !anySession ? "Select at least one session." : null);
        SetError(TrackError, TrackPicker.SelectedIndex < 0 ? "Please select a track." : null);

        if (EventNameError.IsVisible || SessionError.IsVisible || TrackError.IsVisible)
            return;

        NextRequested?.Invoke(this, EventArgs.Empty);
    }

    public void Reset()
    {
        EventNameEntry.Text = string.Empty;
        CbKeynotes.IsChecked = CbWorkshops.IsChecked = CbBreakouts.IsChecked = false;
        TrackPicker.SelectedIndex = -1;
        CbDinner.IsChecked = CbVip.IsChecked = false;
        AttendeeStepper.Value = 1;

        foreach (var lbl in new[] { EventNameError, SessionError, TrackError })
            SetError(lbl, null);
    }
}