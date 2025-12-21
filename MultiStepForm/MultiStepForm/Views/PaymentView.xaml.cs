using System.Text.RegularExpressions;                   

namespace MultiStepForm                                 
{
    public partial class PaymentView : ContentView      
    {
        public event EventHandler? BackRequested;        // Raised when the user taps Back
        public event EventHandler? SubmitRequested;      // Raised after successful validation/submission
        public event EventHandler? StartNewRequested;    // Raised when user starts a new registration
        public event EventHandler? PaymentMethodChanged; // Raised when payment method selection changes

        public PaymentView()                             
        {
            InitializeComponent();                       

            PaymentMethodPicker.ItemsSource = PickerData.PaymentMethods;   // Populate payment-method ComboBox 

            BtnBackToAccommodation.Clicked += (_, __) => BackRequested?.Invoke(this, EventArgs.Empty);   // On Back click, notify parent via BackRequested 

            BtnStartNewRegistration.Clicked += (_, __) => StartNewRequested?.Invoke(this, EventArgs.Empty);   // On Start New click, notify parent to reset flow

            BtnPaymentSubmit.Clicked += OnPaymentSubmitClicked;   // Wire Submit button to validation/submit handler

            PaymentMethodPicker.SelectionChanged += OnPaymentMethodChanged;   // Update UI when payment method changes (e.g., show/hide card fields)
        }

        private void SetError(Label label, string? message) // show/hide error labels
        {
            if (string.IsNullOrWhiteSpace(message))         // No message => hide
            {
                label.IsVisible = false;
                label.Text = "";
            }
            else                                            // Has message => show with text
            {
                label.Text = message;
                label.IsVisible = true;
            }
        }

        private void OnPaymentMethodChanged(object? sender, EventArgs e) // Handles method selection changes
        {
            var method = PaymentMethodPicker.SelectedItem?.ToString() ?? string.Empty;   // Get selected method safely as string

            // Show card fields for non-UPI methods; hide when UPI selected
            CardDetailsStack.IsVisible = !method.Equals("UPI", StringComparison.OrdinalIgnoreCase);

            // If hiding card fields, clear their error states
            if (!CardDetailsStack.IsVisible)
            {
                CardNumberField.HasError = false;
                ExpiryField.HasError = false;
                CvvField.HasError = false;
            }

            PaymentMethodChanged?.Invoke(this, EventArgs.Empty); // Notify subscribers of change
        }

        internal async void OnPaymentSubmitClicked(object? sender, EventArgs e) // Submit button handler
        {
            var method = PaymentMethodPicker.SelectedItem?.ToString();   // Current selected payment method 

            // Validate payment method selection 
            SetError(PaymentMethodError, string.IsNullOrWhiteSpace(method) ? "Select a payment method." : null);

            // Card fields are required when method is not UPI
            bool needsCard = !string.IsNullOrWhiteSpace(method) &&
                             !method.Equals("UPI", StringComparison.OrdinalIgnoreCase);
            
            // Billing address
            BillingField.HasError = string.IsNullOrWhiteSpace(BillingField.Text?.Trim());

            if (needsCard)
            {
                // Card number: remove spaces; require 13–19 digits
                var card = (CardNumberField.Text ?? string.Empty).Replace(" ", "");
                CardNumberField.HasError = !Regex.IsMatch(card, @"^\d{13,19}$");

                // Expiry: MM/YY 
                var expiry = (ExpiryField.Text ?? string.Empty).Trim();
                ExpiryField.HasError = !Regex.IsMatch(expiry, @"^(0[1-9]|1[0-2])\/\d{2}$");

                // CVV: 3 or 4 digits
                var cvv = (CvvField.Text ?? string.Empty).Trim();
                CvvField.HasError = !Regex.IsMatch(cvv, @"^\d{3,4}$");
            }
            else
            {
                // If not a card payment, clear any card-related errors
                CardNumberField.HasError = false;
                ExpiryField.HasError = false;
                CvvField.HasError = false;
            }

            // Aggregate overall validation state
            bool anyError =
                PaymentMethodError.IsVisible ||
                BillingField.HasError ||
                CardNumberField.HasError ||
                ExpiryField.HasError ||
                CvvField.HasError;

            if (anyError) return;                         // Stop submission if any validation failed

            // Success: switch to success panel
            PaymentFormStack.IsVisible = false;           // Hide the form
            SuccessStack.IsVisible = true;                // Show success confirmation

            SubmitRequested?.Invoke(this, EventArgs.Empty); // Notify parent that submission succeeded
        }

        // Called by parent to update the displayed total amount text 
        public void SetEstimatedAmount(string amount)
        {
            EstimatedAmountLabel.Text = amount;          // Update the total label
        }

        // Exposing selected method
        public string? SelectedPaymentMethod => PaymentMethodPicker.SelectedItem?.ToString();   

        // Reset the view to its initial state
        public void Reset()
        {
            PaymentMethodPicker.SelectedItem = null;     
            CardNumberEntry.Text = string.Empty;         
            ExpiryEntry.Text = string.Empty;             
            CvvEntry.Text = string.Empty;                
            BillingAddressEntry.Text = string.Empty;     
            PromoEntry.Text = string.Empty;              
            CardDetailsStack.IsVisible = false;          
            EstimatedAmountLabel.Text = 0.0.ToString("C"); 
            SuccessStack.IsVisible = false;              
            PaymentFormStack.IsVisible = true;       
        }
    }
}