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
        readonly Dictionary<string, double> RoomMultiplier = new() // Room type cost multipliers
        {
            ["Single"] = 1.0,
            ["Double"] = 1.4,
            ["Suite"] = 2.0
        };

        public RegistrationWizard(MainPage page)  
        {
            _page = page; // Store the page reference

            //Find each tab
            _tabView = _page.FindByName<SfTabView>("TabView")!;                  // Find the TabView by name
            _tabPersonal = _page.FindByName<SfTabItem>("TabPersonal")!;         
            _tabEvent = _page.FindByName<SfTabItem>("TabEvent")!;               
            _tabAccommodation = _page.FindByName<SfTabItem>("TabAccommodation")!; 
            _tabPayment = _page.FindByName<SfTabItem>("TabPayment")!;          

            _personalView = _page.FindByName<PersonalInfoView>("PersonalView")!;    
            _eventView = _page.FindByName<EventSelectionView>("EventView")!;        
            _accommodationView = _page.FindByName<AccommodationView>("Accommodation")!; 
            _paymentView = _page.FindByName<PaymentView>("Payment")!;               

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
            var ci = _accommodationView.FindByName<DatePicker>("CheckInPicker");   // Get the Check-In date picker
            var co = _accommodationView.FindByName<DatePicker>("CheckOutPicker");  // Get the Check-Out date picker
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
            var attendeeStepper = _eventView.FindByName<Stepper>("AttendeeStepper"); // Number of attendees control
            var cbWorkshops = _eventView.FindByName<CheckBox>("CbWorkshops");        // Workshops option
            var cbDinner = _eventView.FindByName<CheckBox>("CbDinner");              // Dinner option
            var cbVip = _eventView.FindByName<CheckBox>("CbVip");                    // VIP option

            // Accommodation controls (SfComboBox)
            var hotelPicker = _accommodationView.FindByName<SfComboBox>("HotelPicker");       // Hotel selection
            var roomTypePicker = _accommodationView.FindByName<SfComboBox>("RoomTypePicker"); // Room type selection
            var checkInPicker = _accommodationView.FindByName<DatePicker>("CheckInPicker");   // Check-in date
            var checkOutPicker = _accommodationView.FindByName<DatePicker>("CheckOutPicker"); // Check-out date
            var cbAirportPickup = _accommodationView.FindByName<CheckBox>("CbAirportPickup"); // Airport pickup option
            var cbShuttle = _accommodationView.FindByName<CheckBox>("CbShuttle");             // Shuttle option

            // Payment controls (SfComboBox)
            var paymentMethodPicker = _paymentView.FindByName<SfComboBox>("PaymentMethodPicker"); // Payment method

            int attendees = (int)Math.Round(attendeeStepper?.Value ?? 1); // Attendee count 
            double total = attendees * BaseTicket;                        // Start with base ticket cost

            if (cbWorkshops?.IsChecked == true) total += attendees * WorkshopAddon; // Add workshop fees if selected
            if (cbDinner?.IsChecked == true) total += attendees * DinnerAddon;      // Add dinner fees if selected
            if (cbVip?.IsChecked == true) total += attendees * VipAddon;            // Add VIP fees if selected

            DateTime? checkIn = checkInPicker?.Date;   
            DateTime? checkOut = checkOutPicker?.Date; 
            int nights = Math.Max(0, (int)((checkOut - checkIn)?.TotalDays ?? 0)); 

            // If hotel and room selections are valid and nights > 0, add accommodation cost
            if (hotelPicker?.SelectedItem is string hotel && HotelRate.TryGetValue(hotel, out var rate)
                && roomTypePicker?.SelectedItem is string room && RoomMultiplier.TryGetValue(room, out var mult)
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