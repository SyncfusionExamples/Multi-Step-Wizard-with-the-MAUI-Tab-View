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

            paymentMethodPicker.ItemsSource = PickerData.PaymentMethods;   // Populate payment-method ComboBox 

            btnBackToAccommodation.Clicked += (_, __) => BackRequested?.Invoke(this, EventArgs.Empty);   // On Back click, notify parent via BackRequested 

            btnStartNewRegistration.Clicked += (_, __) => StartNewRequested?.Invoke(this, EventArgs.Empty);   // On Start New click, notify parent to reset flow

            btnPaymentSubmit.Clicked += OnPaymentSubmitClicked;   // Wire Submit button to validation/submit handler

            paymentMethodPicker.SelectionChanged += OnPaymentMethodChanged;   // Update UI when payment method changes (e.g., show/hide card fields)
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
            var method = paymentMethodPicker.SelectedItem?.ToString() ?? string.Empty;   // Get selected method safely as string

            // Show card fields for non-UPI methods; hide when UPI selected
            cardDetailsStack.IsVisible = !method.Equals("UPI", StringComparison.OrdinalIgnoreCase);

            // If hiding card fields, clear their error states
            if (!cardDetailsStack.IsVisible)
            {
                cardNumberField.HasError = false;
                expiryField.HasError = false;
                cvvField.HasError = false;
            }

            PaymentMethodChanged?.Invoke(this, EventArgs.Empty); // Notify subscribers of change
        }

        internal async void OnPaymentSubmitClicked(object? sender, EventArgs e) // Submit button handler
        {
            var method = paymentMethodPicker.SelectedItem?.ToString();   // Current selected payment method 

            // Validate payment method selection 
            SetError(paymentMethodError, string.IsNullOrWhiteSpace(method) ? "Select a payment method." : null);

            // Card fields are required when method is not UPI
            bool needsCard = !string.IsNullOrWhiteSpace(method) &&
                             !method.Equals("UPI", StringComparison.OrdinalIgnoreCase);
            
            // Billing address
            billingField.HasError = string.IsNullOrWhiteSpace(billingField.Text?.Trim());

            if (needsCard)
            {
                // Card number: remove spaces; require 13–19 digits
                var card = (cardNumberField.Text ?? string.Empty).Replace(" ", "");
                cardNumberField.HasError = !Regex.IsMatch(card, @"^\d{13,19}$");

                // Expiry: MM/YY 
                var expiry = (expiryField.Text ?? string.Empty).Trim();
                expiryField.HasError = !Regex.IsMatch(expiry, @"^(0[1-9]|1[0-2])\/\d{2}$");

                // CVV: 3 or 4 digits
                var cvv = (cvvField.Text ?? string.Empty).Trim();
                cvvField.HasError = !Regex.IsMatch(cvv, @"^\d{3,4}$");
            }
            else
            {
                // If not a card payment, clear any card-related errors
                cardNumberField.HasError = false;
                expiryField.HasError = false;
                cvvField.HasError = false;
            }

            // Aggregate overall validation state
            bool anyError =
                paymentMethodError.IsVisible ||
                billingField.HasError ||
                cardNumberField.HasError ||
                expiryField.HasError ||
                cvvField.HasError;

            if (anyError) return;                         // Stop submission if any validation failed

            // Success: switch to success panel
            paymentFormStack.IsVisible = false;           // Hide the form
            successStack.IsVisible = true;                // Show success confirmation

            SubmitRequested?.Invoke(this, EventArgs.Empty); // Notify parent that submission succeeded
        }

        // Called by parent to update the displayed total amount text 
        public void SetEstimatedAmount(string amount)
        {
            estimatedAmountLabel.Text = amount;          // Update the total label
        }

        // Exposing selected method
        public string? SelectedPaymentMethod => paymentMethodPicker.SelectedItem?.ToString();   

        // Reset the view to its initial state
        public void Reset()
        {
            paymentMethodPicker.SelectedItem = null;     
            cardNumberEntry.Text = string.Empty;         
            expiryEntry.Text = string.Empty;             
            cvvEntry.Text = string.Empty;                
            billingAddressEntry.Text = string.Empty;     
            promoEntry.Text = string.Empty;              
            cardDetailsStack.IsVisible = false;          
            estimatedAmountLabel.Text = 0.0.ToString("C"); 
            successStack.IsVisible = false;              
            paymentFormStack.IsVisible = true;       
        }
    }
}