namespace MultiStepForm                                       
{
    public partial class EventSelectionView : ContentView       
    {
        public event EventHandler? BackRequested;               // Event fired when user taps Back
        public event EventHandler? NextRequested;               // Event fired when user completes validation and taps Next

        public EventSelectionView()                             
        {
            InitializeComponent();                              

            TrackPicker.ItemsSource = ComboBoxData.Tracks;         // Populate the track ComboBox from a shared data source

            BtnBackToPersonal.Clicked += (_, __) => BackRequested?.Invoke(this, EventArgs.Empty);
            // When Back is clicked, notify parent to navigate to previous step

            BtnEventNext.Clicked += OnEventNextClicked;         // Wire Next button to validation handler
        }

        void SetError(Label label, string? message)             // Helper to show/hide error labels
        {
            if (string.IsNullOrWhiteSpace(message))             
            {
                label.IsVisible = false; label.Text = "";       // Hide the label and clear text
            }
            else                                                
            {
                label.Text = message; label.IsVisible = true;   // Set error text and show the label
            }
        }

        async void OnEventNextClicked(object? sender, EventArgs e) // Next button click handler 
        {
            // Detect if user left all relevant fields empty to show a prompt
            bool allEmpty =
                string.IsNullOrWhiteSpace(EventNameEntry.Text) &&
                !CbKeynotes.IsChecked && !CbWorkshops.IsChecked && !CbBreakouts.IsChecked &&
                TrackPicker.SelectedItem is null;

            if (allEmpty)
            {
                var page = Application.Current?.Windows.FirstOrDefault()?.Page; // Get current Page
                if (page is not null)
                {
                    await page.DisplayAlertAsync("Missing Information", "Please choose your event selections.", "OK");
                    // Prompt the user to make selections
                }
                return;                                          
            }

            bool anySession = CbKeynotes.IsChecked || CbWorkshops.IsChecked || CbBreakouts.IsChecked;
            
            // Field-level validation with specific error messages
            SetError(EventNameError, string.IsNullOrWhiteSpace(EventNameEntry.Text) ? "Event / conference name is required." : null);
            SetError(SessionError, !anySession ? "Select at least one session." : null);
            SetError(TrackError, TrackPicker.SelectedItem is null ? "Please select a track." : null);

            if (EventNameError.IsVisible || SessionError.IsVisible || TrackError.IsVisible)
                return;                                          // Stay on this step if any validation fails

            NextRequested?.Invoke(this, EventArgs.Empty);       
        }

        public void Reset()                                      // Reset the view to its initial state
        {
            EventNameEntry.Text = string.Empty;                  
            CbKeynotes.IsChecked = CbWorkshops.IsChecked = CbBreakouts.IsChecked = false; 
            TrackPicker.SelectedItem = null;                     
            CbDinner.IsChecked = CbVip.IsChecked = false;        
            AttendeeStepper.Value = 1;                           

            foreach (var lbl in new[] { EventNameError, SessionError, TrackError })
                SetError(lbl, null);                             
        }
    }
}