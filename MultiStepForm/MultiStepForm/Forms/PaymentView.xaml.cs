using System.Text.RegularExpressions;

namespace MultiStepForm;

public partial class PaymentView : ContentView
{
    public event EventHandler? BackRequested;
    public event EventHandler? SubmitRequested;
    public event EventHandler? StartNewRequested;
    public event EventHandler? PaymentMethodChanged;
    public PaymentView()
	{
		InitializeComponent();

        BtnBackToAccommodation.Clicked += (_, __) => BackRequested?.Invoke(this, EventArgs.Empty);
        BtnStartNewRegistration.Clicked += (_, __) => StartNewRequested?.Invoke(this, EventArgs.Empty);
        BtnPaymentSubmit.Clicked += OnPaymentSubmitClicked;
        PaymentMethodPicker.SelectedIndexChanged += OnPaymentMethodChangedInternal;
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

    void OnPaymentMethodChangedInternal(object? sender, EventArgs e)
    {
        var method = PaymentMethodPicker.SelectedItem?.ToString() ?? "";
        CardDetailsStack.IsVisible = !method.Equals("UPI", StringComparison.OrdinalIgnoreCase);
        PaymentMethodChanged?.Invoke(this, EventArgs.Empty);
    }

    internal async void OnPaymentSubmitClicked(object? sender, EventArgs e)
    {
        bool allEmpty =
            PaymentMethodPicker.SelectedIndex < 0 &&
            string.IsNullOrWhiteSpace(BillingAddressEntry.Text) &&
            string.IsNullOrWhiteSpace(PromoEntry.Text) &&
            string.IsNullOrWhiteSpace(CardNumberEntry.Text) &&
            string.IsNullOrWhiteSpace(ExpiryEntry.Text) &&
            string.IsNullOrWhiteSpace(CvvEntry.Text);

        if (allEmpty)
        {
            var page = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (page is not null)
            {
                await page.DisplayAlertAsync("Missing Information", "Please provide payment details.", "OK");
            }
            return;
        }

        var method = PaymentMethodPicker.SelectedItem?.ToString();
        SetError(PaymentMethodError, string.IsNullOrWhiteSpace(method) ? "Select a payment method." : null);

        bool needsCard = !string.IsNullOrWhiteSpace(method) && !method.Equals("UPI", StringComparison.OrdinalIgnoreCase);
        if (needsCard)
        {
            var card = (CardNumberEntry.Text ?? "").Replace(" ", "");
            var cvv = CvvEntry.Text ?? "";
            SetError(CardNumberError, Regex.IsMatch(card, @"^\d{13,19}$") ? null : "Enter a valid card number (13-19 digits).");
            SetError(ExpiryError, Regex.IsMatch(ExpiryEntry.Text ?? "", @"^(0[1-9]|1[0-2])\/\d{2}$") ? null : "Expiry format MM/YY.");
            SetError(CvvError, Regex.IsMatch(cvv, @"^\d{3,4}$") ? null : "CVV must be 3 or 4 digits.");
        }
        else
        {
            SetError(CardNumberError, null);
            SetError(ExpiryError, null);
            SetError(CvvError, null);
        }

        SetError(BillingError, string.IsNullOrWhiteSpace(BillingAddressEntry.Text) ? "Billing address is required." : null);

        bool anyError = PaymentMethodError.IsVisible || BillingError.IsVisible ||
                        CardNumberError.IsVisible || ExpiryError.IsVisible || CvvError.IsVisible;

        if (anyError) return;

        // Show success panel and hide the form
        PaymentFormStack.IsVisible = false;
        SuccessStack.IsVisible = true;

        SubmitRequested?.Invoke(this, EventArgs.Empty);
    }

    public void SetEstimatedAmount(string amount)
    {
        EstimatedAmountLabel.Text = amount;
    }

    public string? SelectedPaymentMethod => PaymentMethodPicker.SelectedItem?.ToString();

    public void Reset()
    {
        PaymentMethodPicker.SelectedIndex = -1;
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