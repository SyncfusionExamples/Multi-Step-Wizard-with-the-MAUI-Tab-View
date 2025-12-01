using System.Text.RegularExpressions;

namespace MultiStepForm
{
    public partial class MainPage : ContentPage
    {
        // Lazy accessors to views by x:Name (no generated fields required)
        PersonalInfoView Personal => this.FindByName<PersonalInfoView>("PersonalInfo");
        EventSelectionView Event => this.FindByName<EventSelectionView>("EventInfo");
        MultiStepForm.AccommodationView AccommodationForm => this.FindByName<MultiStepForm.AccommodationView>("AccommodationInfo");
        MultiStepForm.PaymentView PaymentForm => this.FindByName<MultiStepForm.PaymentView>("PaymentInfo");

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
        public MainPage()
        {
            InitializeComponent();

            this.PersonalView.NextRequested += (_, __) =>
            {
                this.TabEvent.IsVisible = true;
                HideFutureTabsFrom(2);
                this.TabView.SelectedIndex = 1;
            };

            this.EventView.BackRequested += (_, __) => this.TabView.SelectedIndex = 0;
            this.EventView.NextRequested += (_, __) =>

            {
                this.TabAccommodation.IsVisible = true;
                HideFutureTabsFrom(3);
                this.TabView.SelectedIndex = 2;
            };


            this.Accommodation.BackRequested += (_, __) => this.TabView.SelectedIndex = 1;
            this.Accommodation.NextRequested += (_, __) =>
            {
                this.TabPayment.IsVisible = true;
                HideFutureTabsFrom(4);
                this.TabView.SelectedIndex = 3;
                UpdateEstimatedAmount(); // refresh on entering payment
            };

            this.Payment.BackRequested += (_, __) => this.TabView.SelectedIndex = 2;
            this.Payment.PaymentMethodChanged += (_, __) => UpdateEstimatedAmount();
            this.Payment.SubmitRequested += (_, __) =>
            {
                // Persist/submit if needed
            };
            this.Payment.StartNewRequested += (_, __) => ResetWizard();

            // Initialize default dates
            var ci = this.Accommodation.FindByName<DatePicker>("CheckInPicker");
            var co = this.Accommodation.FindByName<DatePicker>("CheckOutPicker");
            if (ci != null) ci.Date = DateTime.Today.AddDays(7);
            if (co != null) co.Date = DateTime.Today.AddDays(10);
        }

        // Utility to hide all future tabs starting from a given index
        void HideFutureTabsFrom(int startIndex)
        {
            var items = new[] { this.TabPersonal, this.TabEvent, this.TabAccommodation, this.TabPayment };
            for (int i = startIndex; i < items.Length; i++)
                items[i].IsVisible = false;
        }

        // Recalculate estimate based on selections across views
        void UpdateEstimatedAmount()
        {
            // Event view controls
            var AttendeeStepper = this.EventView.FindByName<Stepper>("AttendeeStepper");
            var CbWorkshops = this.EventView.FindByName<CheckBox>("CbWorkshops");
            var CbDinner = this.EventView.FindByName<CheckBox>("CbDinner");
            var CbVip = this.EventView.FindByName<CheckBox>("CbVip");

            // Accommodation controls
            var HotelPicker = this.Accommodation.FindByName<Picker>("HotelPicker");
            var RoomTypePicker = this.Accommodation.FindByName<Picker>("RoomTypePicker");
            var CheckInPicker = this.Accommodation.FindByName<DatePicker>("CheckInPicker");
            var CheckOutPicker = this.Accommodation.FindByName<DatePicker>("CheckOutPicker");
            var CbAirportPickup = this.Accommodation.FindByName<CheckBox>("CbAirportPickup");
            var CbShuttle = this.Accommodation.FindByName<CheckBox>("CbShuttle");

            // Payment controls
            var PaymentMethodPicker = this.Payment.FindByName<Picker>("PaymentMethodPicker");

            int attendees = (int)Math.Round(AttendeeStepper?.Value ?? 1);

            double total = attendees * BaseTicket;
            if (CbWorkshops?.IsChecked == true) total += attendees * WorkshopAddon;

            if (CbDinner?.IsChecked == true) total += attendees * DinnerAddon;
            if (CbVip?.IsChecked == true) total += attendees * VipAddon;

            DateTime? checkIn = CheckInPicker?.Date;
            DateTime? checkOut = CheckOutPicker?.Date;
            int nights = Math.Max(0, (int)((checkOut - checkIn)?.TotalDays ?? 0));

            if (HotelPicker?.SelectedItem is string hotel && HotelRate.TryGetValue(hotel, out var rate)
                && RoomTypePicker?.SelectedItem is string room && RoomMultiplier.TryGetValue(room, out var mult)
                && nights > 0)
            {
                total += rate * mult * nights;
            }

            if (CbAirportPickup?.IsChecked == true) total += AirportPickup;
            if (CbShuttle?.IsChecked == true) total += Shuttle;

            var pm = PaymentMethodPicker?.SelectedItem?.ToString() ?? "";
            if (!string.IsNullOrEmpty(pm) && !pm.Equals("UPI", StringComparison.OrdinalIgnoreCase))
            {
                total += Math.Round(total * 0.015, 2); // 1.5% fee
            }

            this.Payment.SetEstimatedAmount(total.ToString("C"));
        }

        // Reset entire flow and all tabs
        void ResetWizard()
        {
            this.PersonalView.Reset();
            this.EventView.Reset();
            this.Accommodation.Reset();
            this.Payment.Reset();

            this.TabPersonal.IsVisible = true;
            this.TabEvent.IsVisible = false;
            this.TabAccommodation.IsVisible = false;
            this.TabPayment.IsVisible = false;
            this.TabView.SelectedIndex = 0;
        }
    }
}
