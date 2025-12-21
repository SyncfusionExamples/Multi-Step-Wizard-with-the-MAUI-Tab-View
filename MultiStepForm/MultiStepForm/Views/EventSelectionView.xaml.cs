using Syncfusion.Maui.Core;                        

namespace MultiStepForm                             // Declares the namespace for this view
{
    public partial class EventSelectionView : ContentView   
    {
        public event EventHandler? BackRequested;   // Event fired when user wants to go back
        public event EventHandler? NextRequested;   // Event fired when user proceeds to the next step

        public EventSelectionView()                 
        {
            InitializeComponent();                  

            TrackPicker.ItemsSource = PickerData.Tracks; // Binds picker items to available tracks 

            BtnBackToPersonal.Clicked += (_, __) => BackRequested?.Invoke(this, EventArgs.Empty);    // On Back button click, raise BackRequested 

            BtnEventNext.Clicked += OnEventNextClicked; // Wire Next button to validation handler

            // When any session checkbox changes, update session selection state 
            keynotes.StateChanged += (_, __) => OnSessionSelectionUpdated();
            workshops.StateChanged += (_, __) => OnSessionSelectionUpdated();
            breakouts.StateChanged += (_, __) => OnSessionSelectionUpdated();
        }

        private void FieldNullCheck(SfTextInputLayout inputLayout) // Validates that an input field is not empty
        {
            var text = inputLayout.Text?.Trim();                   // Read and trim input text 
            inputLayout.HasError = string.IsNullOrWhiteSpace(text);// Flag error if empty/whitespace to trigger UI ErrorText
        }

        private void SetError(Label label, string? message)        // Utility to show/hide an error label with a message
        {
            if (string.IsNullOrWhiteSpace(message))                // No message => hide the label
            {
                label.IsVisible = false;
                label.Text = string.Empty;
            }
            else                                                   // Message provided => show label and set text
            {
                label.Text = message;
                label.IsVisible = true;
            }
        }

        private bool AnySessionSelected() =>                       // Checks if at least one session checkbox is selected
            new[] { keynotes, workshops, breakouts }.Any(cb => cb.IsChecked == true);

        // When user toggles any session checkbox, hide the error
        private void OnSessionSelectionUpdated()
        {
            if (AnySessionSelected())                              // If one is selected, clear session error
            {
                SetError(SessionError, null);
            }
        }

        private void OnEventNextClicked(object? sender, EventArgs e) // Handler for Next button click
        {
            FieldNullCheck(Event);                                 // Validate event name 
            bool anySession = AnySessionSelected();                 // Check if any session selected
            SetError(SessionError, anySession ? null : "Select at least one session.");   // Show session error only if none selected
            SetError(TrackError, TrackPicker.SelectedItem is null ? "Please select a track." : null);   // Show track error if no track is selected
            bool hasAnyError = Event.HasError || SessionError.IsVisible || TrackError.IsVisible;    // Aggregate validation state from input layout and error labels
            if (hasAnyError) return;                                // Stop if any validation failed
            NextRequested?.Invoke(this, EventArgs.Empty);           // Otherwise, notify parent to go to next step
        }

        // Reset the view to its initial state
        public void Reset()
        {
            EventNameEntry.Text = string.Empty;                   
            keynotes.IsChecked = workshops.IsChecked = breakouts.IsChecked = false;
            TrackPicker.SelectedItem = null;                        
            networkingDinner.IsChecked = vipAccess.IsChecked = false;
            AttendeeStepper.Value = 1;                              
        }
    }
}