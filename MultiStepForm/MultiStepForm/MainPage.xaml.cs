using System.Text.RegularExpressions;

namespace MultiStepForm
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
            TabView.SelectedIndex = 0;
        }

        // Pricing (sample values)
        const double BaseTicket = 199.0;   // per attendee
        const double WorkshopAddon = 99.0; // per attendee if workshops selected
        const double DinnerAddon = 75.0;   // per attendee
        const double VipAddon = 150.0;     // per attendee
        const double AirportPickup = 60.0; // flat
        const double Shuttle = 30.0;       // flat

        // Hotel nightly rates
        readonly Dictionary<string, double> HotelRate = new()
        {
            ["Hotel Aurora"] = 160,
            ["Cityscape Suites"] = 220,
            ["Grand Meridian"] = 280,
            ["Harbor View"] = 190
        };
        // Room multiplier
        readonly Dictionary<string, double> RoomMultiplier = new()
        {
            ["Single"] = 1.0,
            ["Double"] = 1.4,
            ["Suite"] = 2.0
        };

        // Utility to hide all future tabs starting from a given index
        void HideFutureTabsFrom(int startIndex)
        {
            var items = new[] { TabPersonal, TabEvent, TabAccommodation, TabPayment };
            for (int i = startIndex; i < items.Length; i++)
                items[i].IsVisible = false;
        }

        static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            email = email.Trim().ToLowerInvariant();
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
        }

        static bool IsValidPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            // Accept +, spaces, hyphens, digits; 7-10 digits total
            var digits = Regex.Replace(phone, @"\D", "");
            return digits.Length >= 7 && digits.Length <= 10;
        }

        // Inline error helpers
        void SetError(Label label, string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                label.IsVisible = false;
                label.Text = "";
            }
            else
            {
                label.Text = message;
                label.IsVisible = true;
            }
        }

        // TAB 1 -> 2
        async void OnPersonalNextClicked(object sender, EventArgs e)
        {
            // If whole form is empty, alert and return
            bool allEmpty =
                string.IsNullOrWhiteSpace(FullNameEntry.Text) &&
                string.IsNullOrWhiteSpace(EmailEntry.Text) &&
                string.IsNullOrWhiteSpace(MobileEntry.Text) &&
                string.IsNullOrWhiteSpace(OrgEntry.Text) &&
                string.IsNullOrWhiteSpace(JobTitleEntry.Text);

            if (allEmpty)
            {
                await DisplayAlertAsync("Missing Information", "Please fill the form to proceed.", "OK");
                return;
            }

            // Inline validation
            SetError(FullNameError, string.IsNullOrWhiteSpace(FullNameEntry.Text) ? "Full name is required." : null);
            SetError(EmailError, !IsValidEmail(EmailEntry.Text) ? "Enter a valid email address." : null);
            SetError(MobileError, !IsValidPhone(MobileEntry.Text) ? "Enter a valid phone number." : null);
            SetError(OrgError, string.IsNullOrWhiteSpace(OrgEntry.Text) ? "Organization is required." : null);
            SetError(JobTitleError, string.IsNullOrWhiteSpace(JobTitleEntry.Text) ? "Job title is required." : null);

            bool anyError = FullNameError.IsVisible || EmailError.IsVisible || MobileError.IsVisible ||
                            OrgError.IsVisible || JobTitleError.IsVisible;

            if (anyError) return;

            TabEvent.IsVisible = true;
            HideFutureTabsFrom(2);
            TabView.SelectedIndex = 1;
        }

        void OnBackToPersonalClicked(object sender, EventArgs e) => TabView.SelectedIndex = 0;

        // TAB 2 -> 3
        async void OnEventNextClicked(object sender, EventArgs e)
        {
            // Alert only if the whole section is empty
            bool allEmpty =
                string.IsNullOrWhiteSpace(EventNameEntry.Text) &&
                !CbKeynotes.IsChecked && !CbWorkshops.IsChecked && !CbBreakouts.IsChecked &&
                TrackPicker.SelectedIndex < 0;

            if (allEmpty)
            {
                await DisplayAlertAsync("Missing Information", "Please choose your event selections.", "OK");
                return;
            }

            // Inline validation
            bool anySession = CbKeynotes.IsChecked || CbWorkshops.IsChecked || CbBreakouts.IsChecked;

            SetError(EventNameError, string.IsNullOrWhiteSpace(EventNameEntry.Text) ? "Event / conference name is required." : null);
            SetError(SessionError, !anySession ? "Select at least one session." : null);
            SetError(TrackError, TrackPicker.SelectedIndex < 0 ? "Please select a track." : null);

            if (EventNameError.IsVisible || SessionError.IsVisible || TrackError.IsVisible)
                return;

            TabAccommodation.IsVisible = true;
            HideFutureTabsFrom(3);
            TabView.SelectedIndex = 2;
        }

        void OnBackToEventClicked(object sender, EventArgs e) => TabView.SelectedIndex = 1;

        // TAB 3 -> 4
        async void OnAccommodationNextClicked(object sender, EventArgs e)
        {
            // Alert only if nothing chosen in this tab
            bool allEmpty =
                HotelPicker.SelectedIndex < 0 &&
                RoomTypePicker.SelectedIndex < 0 &&
                string.IsNullOrWhiteSpace(SpecialReqEntry.Text) &&
                !CbAirportPickup.IsChecked && !CbShuttle.IsChecked;

            if (allEmpty)
            {
                await DisplayAlertAsync("Missing Information", "Please provide accommodation preferences.", "OK");
                return;
            }

            // Inline validation (no date/time validation)
            SetError(HotelError, HotelPicker.SelectedIndex < 0 ? "Choose a hotel." : null);
            SetError(RoomTypeError, RoomTypePicker.SelectedIndex < 0 ? "Choose a room type." : null);

            if (HotelError.IsVisible || RoomTypeError.IsVisible)
                return;

            // Update estimated total
            UpdateEstimatedAmount();

            TabPayment.IsVisible = true;
            HideFutureTabsFrom(4);
            TabView.SelectedIndex = 3;
        }

        void OnBackToAccommodationClicked(object sender, EventArgs e) => TabView.SelectedIndex = 2;

        // Payment method toggle for card details (show for any method except UPI)
        void OnPaymentMethodChanged(object sender, EventArgs e)
        {
            var method = PaymentMethodPicker.SelectedItem?.ToString() ?? "";
            CardDetailsStack.IsVisible = !method.Equals("UPI", StringComparison.OrdinalIgnoreCase);
            UpdateEstimatedAmount();
        }

        // Recalculate estimate based on selections
        void UpdateEstimatedAmount()
        {
            int attendees = (int)Math.Round(AttendeeStepper.Value);

            // Base and sessions
            double total = attendees * BaseTicket;
            if (CbWorkshops.IsChecked) total += attendees * WorkshopAddon;

            // Add-ons per attendee
            if (CbDinner.IsChecked) total += attendees * DinnerAddon;
            if (CbVip.IsChecked) total += attendees * VipAddon;

            // Accommodation (approximate nights; DatePicker.Date is non-nullable)
            DateTime? checkIn = CheckInPicker?.Date;
            DateTime? checkOut = CheckOutPicker?.Date;

            int nights = Math.Max(0, (int)((checkOut - checkIn)?.TotalDays ?? 0));
            if (HotelPicker.SelectedItem is string hotel && HotelRate.TryGetValue(hotel, out var rate)
                && RoomTypePicker.SelectedItem is string room && RoomMultiplier.TryGetValue(room, out var mult)
                && nights > 0)
            {
                total += rate * mult * nights;
            }

            // Transport
            if (CbAirportPickup.IsChecked) total += AirportPickup;
            if (CbShuttle.IsChecked) total += Shuttle;

            // Optional processing fee for non-UPI methods
            var pm = PaymentMethodPicker.SelectedItem?.ToString() ?? "";
            if (!string.IsNullOrEmpty(pm) && !pm.Equals("UPI", StringComparison.OrdinalIgnoreCase))
            {
                total += Math.Round(total * 0.015, 2); // 1.5% fee
            }

            EstimatedAmountLabel.Text = total.ToString("C");
        }

        // SUBMIT on Payment tab
        async void OnPaymentSubmitClicked(object sender, EventArgs e)
        {
            // Alert if entire payment section empty
            bool allEmpty =
                PaymentMethodPicker.SelectedIndex < 0 &&
                string.IsNullOrWhiteSpace(BillingAddressEntry.Text) &&
                string.IsNullOrWhiteSpace(PromoEntry.Text) &&
                string.IsNullOrWhiteSpace(CardNumberEntry.Text) &&
                string.IsNullOrWhiteSpace(ExpiryEntry.Text) &&
                string.IsNullOrWhiteSpace(CvvEntry.Text);

            if (allEmpty)
            {
                await DisplayAlertAsync("Missing Information", "Please provide payment details.", "OK");
                return;
            }

            // Inline validation (no date/time validation)
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

            // TODO: Persist/submit data here

            // Show success panel and hide the form
            PaymentFormStack.IsVisible = false;
            SuccessStack.IsVisible = true;
        }

        // Restart flow
        void OnStartNewRegistrationClicked(object sender, EventArgs e)
        {
            ResetWizard();
        }

        // Reset flow
        void ResetWizard()
        {
            // Clear personal
            FullNameEntry.Text = EmailEntry.Text = MobileEntry.Text = OrgEntry.Text = JobTitleEntry.Text = string.Empty;

            // Event
            EventNameEntry.Text = string.Empty;
            CbKeynotes.IsChecked = CbWorkshops.IsChecked = CbBreakouts.IsChecked = false;
            TrackPicker.SelectedIndex = -1;
            CbDinner.IsChecked = CbVip.IsChecked = false;
            AttendeeStepper.Value = 1;

            // Accommodation
            HotelPicker.SelectedIndex = -1;
            RoomTypePicker.SelectedIndex = -1;
            CheckInPicker.Date = DateTime.Today.AddDays(7);
            CheckOutPicker.Date = DateTime.Today.AddDays(10);
            SpecialReqEntry.Text = string.Empty;
            CbAirportPickup.IsChecked = CbShuttle.IsChecked = false;

            // Payment
            PaymentMethodPicker.SelectedIndex = -1;
            CardNumberEntry.Text = ExpiryEntry.Text = CvvEntry.Text = string.Empty;
            BillingAddressEntry.Text = PromoEntry.Text = string.Empty;
            CardDetailsStack.IsVisible = false;
            EstimatedAmountLabel.Text = 0.0.ToString("C");

            // Hide success, show form
            SuccessStack.IsVisible = false;
            PaymentFormStack.IsVisible = true;

            // Clear errors
            foreach (var lbl in new[] { FullNameError, EmailError, MobileError, OrgError, JobTitleError,
                                    EventNameError, SessionError, TrackError,
                                    PaymentMethodError, CardNumberError, ExpiryError, CvvError, BillingError })
            {
                SetError(lbl, null);
            }

            // Visibility
            TabPersonal.IsVisible = true;
            TabEvent.IsVisible = false;
            TabAccommodation.IsVisible = false;
            TabPayment.IsVisible = false;

            TabView.SelectedIndex = 0;
        }
    }
}
