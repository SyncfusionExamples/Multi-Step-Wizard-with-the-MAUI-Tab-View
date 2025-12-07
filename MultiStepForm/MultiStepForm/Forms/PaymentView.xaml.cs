using System.Text.RegularExpressions;

namespace MultiStepForm
{
    public partial class PaymentView : ContentView
    {
        public event EventHandler? BackRequested;          // Raised when the user taps Back
        public event EventHandler? SubmitRequested;        // Raised after successful validation/submission
        public event EventHandler? StartNewRequested;      // Raised when user starts a new registration
        public event EventHandler? PaymentMethodChanged;   // Raised when payment method selection changes

        public PaymentView()                               
        {
            InitializeComponent();                         

            PaymentMethodPicker.ItemsSource = ComboBoxData.PaymentMethods;          // Populate payment method ComboBox

            BtnBackToAccommodation.Clicked += (_, __) => BackRequested?.Invoke(this, EventArgs.Empty);
            // Forward Back button click to parent via BackRequested event

            BtnStartNewRegistration.Clicked += (_, __) => StartNewRequested?.Invoke(this, EventArgs.Empty);
            // Forward "Start New Registration" button click

            BtnPaymentSubmit.Clicked += OnPaymentSubmitClicked;                  // Wire Submit button to handler
            PaymentMethodPicker.SelectionChanged += OnPaymentMethodChangedInternal; // React to payment method changes
        }

        void SetError(Label label, string? message)        //Show/hide an error label
        {
            if (string.IsNullOrWhiteSpace(message))        // No error -> hide label and clear text
            {
                label.IsVisible = false; label.Text = "";
            }
            else                                            // Error -> set text and show label
            {
                label.Text = message; label.IsVisible = true;
            }
        }

        void OnPaymentMethodChangedInternal(object? sender, EventArgs e)
        {
            var method = PaymentMethodPicker.SelectedItem?.ToString() ?? "";               // Current method as string
            CardDetailsStack.IsVisible = !method.Equals("UPI", StringComparison.OrdinalIgnoreCase);
            // Show card fields for non-UPI methods, hide for UPI

            PaymentMethodChanged?.Invoke(this, EventArgs.Empty);                          
        }

        internal async void OnPaymentSubmitClicked(object? sender, EventArgs e)
        {
            // If user left everything empty, show a prompt
            bool allEmpty =
                PaymentMethodPicker.SelectedItem is null &&
                string.IsNullOrWhiteSpace(BillingAddressEntry.Text) &&
                string.IsNullOrWhiteSpace(PromoEntry.Text) &&
                string.IsNullOrWhiteSpace(CardNumberEntry.Text) &&
                string.IsNullOrWhiteSpace(ExpiryEntry.Text) &&
                string.IsNullOrWhiteSpace(CvvEntry.Text);

            if (allEmpty)
            {
                var page = Application.Current?.Windows.FirstOrDefault()?.Page;            // Get current Page 
                if (page is not null)
                {
                    await page.DisplayAlertAsync("Missing Information", "Please provide payment details.", "OK");
                    // Prompt the user to enter payment info
                }
                return;                                                                    
            }

            var method = PaymentMethodPicker.SelectedItem?.ToString();                      // Selected method 
            SetError(PaymentMethodError, string.IsNullOrWhiteSpace(method) ? "Select a payment method." : null);

            bool needsCard = !string.IsNullOrWhiteSpace(method) && !method.Equals("UPI", StringComparison.OrdinalIgnoreCase);
            // Card details required for all methods except UPI

            if (needsCard)
            {
                var card = (CardNumberEntry.Text ?? "").Replace(" ", "");                  // Strip spaces from card number
                var cvv = CvvEntry.Text ?? "";

                // Validate card number: 13–19 digits
                SetError(CardNumberError, Regex.IsMatch(card, @"^\d{13,19}$") ? null : "Enter a valid card number (13-19 digits).");

                // Validate expiry in MM/YY format, with month 01–12
                SetError(ExpiryError, Regex.IsMatch(ExpiryEntry.Text ?? "", @"^(0[1-9]|1[0-2])\/\d{2}$") ? null : "Expiry format MM/YY.");

                // Validate CVV: 3 or 4 digits
                SetError(CvvError, Regex.IsMatch(cvv, @"^\d{3,4}$") ? null : "CVV must be 3 or 4 digits.");
            }
            else
            {
                // If card not needed (UPI), ensure card errors are cleared and hidden
                SetError(CardNumberError, null);
                SetError(ExpiryError, null);
                SetError(CvvError, null);
            }

            SetError(BillingError, string.IsNullOrWhiteSpace(BillingAddressEntry.Text) ? "Billing address is required." : null);
            // Billing address is required regardless of method

            bool anyError = PaymentMethodError.IsVisible || BillingError.IsVisible ||
                            CardNumberError.IsVisible || ExpiryError.IsVisible || CvvError.IsVisible;
            // Check if any error labels are visible

            if (anyError) return;                                                           // Stop if validation failed

            PaymentFormStack.IsVisible = false;                                             // Hide form
            SuccessStack.IsVisible = true;                                                  // Show success panel
            SubmitRequested?.Invoke(this, EventArgs.Empty);                                 // Notify parent of submission
        }

        // Called by parent to update the displayed total
        public void SetEstimatedAmount(string amount)   
        {
            EstimatedAmountLabel.Text = amount;         
        }

        public string? SelectedPaymentMethod => PaymentMethodPicker.SelectedItem?.ToString();
        // Convenience property to fetch selected payment method

        // Reset the view to initial state
        public void Reset()                              
        {
            PaymentMethodPicker.SelectedItem = null;     
            CardNumberEntry.Text = ExpiryEntry.Text = CvvEntry.Text = string.Empty; 
            BillingAddressEntry.Text = PromoEntry.Text = string.Empty;              
            CardDetailsStack.IsVisible = false;          
            EstimatedAmountLabel.Text = 0.0.ToString("C"); 
            SuccessStack.IsVisible = false;              
            PaymentFormStack.IsVisible = true;          

            foreach (var lbl in new[] { PaymentMethodError, CardNumberError, ExpiryError, CvvError, BillingError })
                SetError(lbl, null);                     
        }
    }
}