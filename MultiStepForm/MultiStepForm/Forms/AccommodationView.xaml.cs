namespace MultiStepForm;

public partial class AccommodationView : ContentView
{
    public event EventHandler? BackRequested;
    public event EventHandler? NextRequested;
    public AccommodationView()
	{
		InitializeComponent();
        
        BtnBackToEvent.Clicked += (_, __) => BackRequested?.Invoke(this, EventArgs.Empty);
        BtnAccommodationNext.Clicked += OnAccommodationNextClicked;
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

    internal async void OnAccommodationNextClicked(object? sender, EventArgs e)
    {
        bool allEmpty =
            HotelPicker.SelectedIndex < 0 &&
            RoomTypePicker.SelectedIndex < 0 &&
            string.IsNullOrWhiteSpace(SpecialReqEntry.Text) &&
            !CbAirportPickup.IsChecked && !CbShuttle.IsChecked;

        if (allEmpty)
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is not null)
            {
                await page.DisplayAlertAsync("Missing Information", "Please provide accommodation preferences.", "OK");
            }
            return;
        }
        
        SetError(HotelError, HotelPicker.SelectedIndex < 0 ? "Choose a hotel." : null);
        SetError(RoomTypeError, RoomTypePicker.SelectedIndex < 0 ? "Choose a room type." : null);

        if (HotelError.IsVisible || RoomTypeError.IsVisible)
            return;

        NextRequested?.Invoke(this, EventArgs.Empty);
    }

    public void Reset()
    {
        HotelPicker.SelectedIndex = -1;
        RoomTypePicker.SelectedIndex = -1;
        CheckInPicker.Date = DateTime.Today;
        CheckOutPicker.Date = DateTime.Today;
        SpecialReqEntry.Text = string.Empty;
        CbAirportPickup.IsChecked = CbShuttle.IsChecked = false;

        foreach (var lbl in new[] { HotelError, RoomTypeError })
            SetError(lbl, null);
    }
}