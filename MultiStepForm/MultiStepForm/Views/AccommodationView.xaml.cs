namespace MultiStepForm                           
{
    public partial class AccommodationView : ContentView  
    {
        public event EventHandler? BackRequested;  // Event raised when user clicks Back 
        public event EventHandler? NextRequested;  // Event raised when validation passes and user clicks Next

        public AccommodationView()                 
        {
            InitializeComponent();                

            HotelPicker.ItemsSource = PickerData.Hotels;       // Set hotel combo's data source list
            RoomTypePicker.ItemsSource = PickerData.RoomTypes; // Set room type combo's data source list

            BtnBackToEvent.Clicked += (_, __) => BackRequested?.Invoke(this, EventArgs.Empty);   // On Back button click, raise BackRequested 

            BtnAccommodationNext.Clicked += OnAccommodationNextClicked;   // Wire Next button click to validation handler
        }

        void SetError(Label label, string? message) // Show or hide an error label 
        {
            if (string.IsNullOrWhiteSpace(message)) // If no error message provided
            {
                label.IsVisible = false; label.Text = ""; // Hide the label and clear text
            }
            else                                     // Error message provided
            {
                label.Text = message; label.IsVisible = true; // Set message and show the label
            }
        }

        internal async void OnAccommodationNextClicked(object? sender, EventArgs e) // Next button click handler
        {
            SetError(HotelError, HotelPicker.SelectedItem is null ? "Choose a hotel." : null);   // If no hotel selected, show error; otherwise, clear it

            SetError(RoomTypeError, RoomTypePicker.SelectedItem is null ? "Choose a room type." : null);   // If no room type selected, show error; otherwise, clear it

            if (HotelError.IsVisible || RoomTypeError.IsVisible)
                return; // If any required field is invalid, do not proceed

            NextRequested?.Invoke(this, EventArgs.Empty); // Notify parent to move to next step
        }

        public void Reset() // Reset the view to its initial state
        {
            HotelPicker.SelectedItem = null;           
            RoomTypePicker.SelectedItem = null;        
            CheckInPicker.Date = DateTime.Today;       
            CheckOutPicker.Date = DateTime.Today;      
            SpecialReqEntry.Text = string.Empty;       
            airportPickup.IsChecked = shuttle.IsChecked = false; 
        }
    }
}