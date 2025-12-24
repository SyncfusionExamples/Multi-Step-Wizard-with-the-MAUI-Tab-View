using Syncfusion.Maui.Buttons;
using Syncfusion.Maui.Inputs;           
using Syncfusion.Maui.TabView;          

namespace MultiStepForm                
{
    public class RegistrationWizard     // To manage the multi-step form
    {
        // References
        private readonly MainPage _page;               

        // Tabs and tab view
        private readonly SfTabView _tabView;           
        private readonly SfTabItem _tabPersonal;       
        private readonly SfTabItem _tabEvent;         
        private readonly SfTabItem _tabAccommodation;  
        private readonly SfTabItem _tabPayment;        

        // Views inside tabs
        private readonly PersonalInfoView _personalView;        
        private readonly EventSelectionView _eventView;          
        private readonly AccommodationView _accommodationView;   
        private readonly PaymentView _paymentView;               

        // Pricing (sample values)
        const double BaseTicket = 199.0;  
        const double WorkshopAddon = 99.0; 
        const double DinnerAddon = 75.0;   
        const double VipAddon = 150.0;     
        const double AirportPickup = 60.0; 
        const double Shuttle = 30.0;       

        // Hotel rates
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

        public RegistrationWizard(MainPage page)  
        {
            _page = page; // Store the page reference

            //Find each tab
            _tabView = _page.FindByName<SfTabView>("tabView")!;                  // Find the TabView by name
            _tabPersonal = _page.FindByName<SfTabItem>("tabPersonal")!;         
            _tabEvent = _page.FindByName<SfTabItem>("tabEvent")!;               
            _tabAccommodation = _page.FindByName<SfTabItem>("tabAccommodation")!; 
            _tabPayment = _page.FindByName<SfTabItem>("tabPayment")!;          

            _personalView = _page.FindByName<PersonalInfoView>("personalView")!;    
            _eventView = _page.FindByName<EventSelectionView>("eventView")!;        
            _accommodationView = _page.FindByName<AccommodationView>("accommodation")!; 
            _paymentView = _page.FindByName<PaymentView>("payment")!;               

            WireEvents();         // Set up event handlers for navigation and updates
            InitializeDefaults(); // Initialize default values (e.g., dates)
        }

        private void WireEvents() // Subscribes to events exposed by the child views
        {
            _personalView.NextRequested += (_, __) =>           // When user clicks Next on Personal tab
            {
                _tabEvent.IsVisible = true;                    // Show the Event tab
                HideFutureTabsFrom(2);                         // Hide tabs after Event 
                _tabView.SelectedIndex = 1;                    // Navigate to the Event tab 
            };

            _eventView.BackRequested += (_, __) => _tabView.SelectedIndex = 0; // Back from Event to Personal

            _eventView.NextRequested += (_, __) =>             // Next from Event tab
            {
                _tabAccommodation.IsVisible = true;            // Show Accommodation tab
                HideFutureTabsFrom(3);                         // Hide tabs after Accommodation
                _tabView.SelectedIndex = 2;                    // Navigate to Accommodation tab
            };

            _accommodationView.BackRequested += (_, __) => _tabView.SelectedIndex = 1; // Back to Event tab

            _accommodationView.NextRequested += (_, __) =>     // Next from Accommodation
            {
                _tabPayment.IsVisible = true;                  // Show Payment tab
                HideFutureTabsFrom(4);                         // Hide future tabs 
                _tabView.SelectedIndex = 3;                    // Navigate to Payment tab
                UpdateEstimatedAmount();                       // Refresh estimate upon entering Payment
            };

            _paymentView.BackRequested += (_, __) => _tabView.SelectedIndex = 2;     // Back to Accommodation
            _paymentView.PaymentMethodChanged += (_, __) => UpdateEstimatedAmount(); // Recompute fees on method change

            _paymentView.SubmitRequested += (_, __) =>         // When user submits payment
            {
                // Placeholder for saving or processing
            };

            _paymentView.StartNewRequested += (_, __) => ResetWizard(); // Start a new registration flow
        }

        private void InitializeDefaults() // Sets initial values for specific controls
        {
            var ci = _accommodationView.FindByName<DatePicker>("checkInPicker");   // Get the Check-In date picker
            var co = _accommodationView.FindByName<DatePicker>("checkOutPicker");  // Get the Check-Out date picker
            if (ci != null) ci.Date = DateTime.Today;   
            if (co != null) co.Date = DateTime.Today;  
        }

        // Hide all future tabs starting from a given index 
        private void HideFutureTabsFrom(int startIndex) 
        {
            var items = new[] { _tabPersonal, _tabEvent, _tabAccommodation, _tabPayment }; // Ordered tabs
            for (int i = startIndex; i < items.Length; i++)  // Loop from start index to end
                items[i].IsVisible = false;                  // Hide each subsequent tab
        }

        // Recalculate estimate based on selections across views
        private void UpdateEstimatedAmount()
        {
            // Event view controls
            var numericentry = _eventView.FindByName<SfNumericEntry>("numericEntry"); // Number of attendees control
            var cbWorkshops = _eventView.FindByName<SfCheckBox>("workshops");        // Workshops option
            var cbDinner = _eventView.FindByName<SfCheckBox>("networkingDinner");              // Dinner option
            var cbVip = _eventView.FindByName<SfCheckBox>("vipAccess");                    // VIP option

            // Accommodation controls (SfComboBox)
            var hotelpicker = _accommodationView.FindByName<SfComboBox>("hotelPicker");       // Hotel selection
            var roomtypePicker = _accommodationView.FindByName<SfComboBox>("roomTypePicker"); // Room type selection
            var checkinPicker = _accommodationView.FindByName<DatePicker>("checkInPicker");   // Check-in date
            var checkoutPicker = _accommodationView.FindByName<DatePicker>("checkOutPicker"); // Check-out date
            var cbAirportPickup = _accommodationView.FindByName<SfCheckBox>("airportPickup"); // Airport pickup option
            var cbShuttle = _accommodationView.FindByName<SfCheckBox>("shuttle");             // Shuttle option

            // Payment controls (SfComboBox)
            var paymentMethodPicker = _paymentView.FindByName<SfComboBox>("paymentMethodPicker"); // Payment method

            int attendees = (int)Math.Round(numericentry?.Value ?? 1); // Attendee count 
            double total = attendees * BaseTicket;                        // Start with base ticket cost

            if (cbWorkshops?.IsChecked == true) total += attendees * WorkshopAddon; // Add workshop fees if selected
            if (cbDinner?.IsChecked == true) total += attendees * DinnerAddon;      // Add dinner fees if selected
            if (cbVip?.IsChecked == true) total += attendees * VipAddon;            // Add VIP fees if selected

            DateTime? checkIn = checkinPicker?.Date;   
            DateTime? checkOut = checkoutPicker?.Date; 
            int nights = Math.Max(0, (int)((checkOut - checkIn)?.TotalDays ?? 0)); 

            // If hotel and room selections are valid and nights > 0, add accommodation cost
            if (hotelpicker?.SelectedItem is string hotel && HotelRate.TryGetValue(hotel, out var rate)
                && roomtypePicker?.SelectedItem is string room && RoomMultiplier.TryGetValue(room, out var mult)
                && nights > 0)
            {
                total += rate * mult * nights; // Calculate room cost
            }

            if (cbAirportPickup?.IsChecked == true) total += AirportPickup; // Add airport pickup fee
            if (cbShuttle?.IsChecked == true) total += Shuttle;             // Add shuttle fee

            var pm = paymentMethodPicker?.SelectedItem?.ToString() ?? "";   // Selected payment method as string

            // Apply a 1.5% processing fee for non-UPI methods
            if (!string.IsNullOrEmpty(pm) && !pm.Equals("UPI", StringComparison.OrdinalIgnoreCase))
            {
                total += Math.Round(total * 0.015, 2); // Add rounded processing fee
            }

            _paymentView.SetEstimatedAmount(total.ToString("C")); // Update UI with currency-formatted total
        }

        // Reset entire flow and all tabs
        private void ResetWizard()
        {
            _personalView.Reset();        
            _eventView.Reset();           
            _accommodationView.Reset();   
            _paymentView.Reset();         

            _tabPersonal.IsVisible = true;      
            _tabEvent.IsVisible = false;        
            _tabAccommodation.IsVisible = false;
            _tabPayment.IsVisible = false;      
            _tabView.SelectedIndex = 0;         
        }
    }
}