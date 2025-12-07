namespace MultiStepForm
{
    public partial class AccommodationView : ContentView
    {
        public event EventHandler? BackRequested;  // Event raised when user clicks Back
        public event EventHandler? NextRequested;  // Event raised when user passes validation and clicks Next

        public AccommodationView()                 
        {
            InitializeComponent();                 

            HotelPicker.ItemsSource = ComboBoxData.Hotels;      // Populate hotel ComboBox with a list
            RoomTypePicker.ItemsSource = ComboBoxData.RoomTypes; // Populate room type ComboBox with a list

            BtnBackToEvent.Clicked += (_, __) => BackRequested?.Invoke(this, EventArgs.Empty);
            // When Back is clicked, raise BackRequested to let the parent navigate to previous step

            BtnAccommodationNext.Clicked += OnAccommodationNextClicked;
            // When Next is clicked, run validation and possibly raise NextRequested
        }

        void SetError(Label label, string? message) // To show/hide an error label
        {
            if (string.IsNullOrWhiteSpace(message)) // No error -> hide
            {
                label.IsVisible = false; label.Text = "";
            }
            else                                     // Error -> set message and show
            {
                label.Text = message; label.IsVisible = true;
            }
        }

        internal async void OnAccommodationNextClicked(object? sender, EventArgs e)
        {
            // If every relevant field is empty/unchecked, prompt the user
            bool allEmpty =
                HotelPicker.SelectedItem is null &&
                RoomTypePicker.SelectedItem is null &&
                string.IsNullOrWhiteSpace(SpecialReqEntry.Text) &&
                !CbAirportPickup.IsChecked && !CbShuttle.IsChecked;

            if (allEmpty)
            {
                var page = Application.Current?.Windows.FirstOrDefault()?.Page; // Get the active Page
                if (page is not null)
                {
                    await page.DisplayAlertAsync("Missing Information", "Please provide accommodation preferences.", "OK");
                    // Show an alert asking user to fill in accommodation preferences
                }

                return; 
            }

            // Validate required selections and set error messages if missing
            SetError(HotelError, HotelPicker.SelectedItem is null ? "Choose a hotel." : null);
            SetError(RoomTypeError, RoomTypePicker.SelectedItem is null ? "Choose a room type." : null);

            if (HotelError.IsVisible || RoomTypeError.IsVisible)
                return; // If any required field is invalid, stay on this step

            NextRequested?.Invoke(this, EventArgs.Empty); // Notify parent to proceed to next step
        }

        public void Reset() // Reset the view to its initial state
        {
            HotelPicker.SelectedItem = null;          
            RoomTypePicker.SelectedItem = null;       
            CheckInPicker.Date = DateTime.Today;      
            CheckOutPicker.Date = DateTime.Today;     
            SpecialReqEntry.Text = string.Empty;     
            CbAirportPickup.IsChecked = CbShuttle.IsChecked = false; 

            foreach (var lbl in new[] { HotelError, RoomTypeError })
                SetError(lbl, null); // Hide and clear error labels
        }
    }
}