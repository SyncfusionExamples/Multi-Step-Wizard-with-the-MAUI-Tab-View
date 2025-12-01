using System;
using System.Collections.Generic;
using System.Text;

namespace MultiStepForm
{
    public class WizardState
    {
        // Tab 1: Personal
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Mobile { get; set; } = "";
        public string Organization { get; set; } = "";
        public string JobTitle { get; set; } = "";

        // Tab 2: Event
        public string EventName { get; set; } = "";
        public bool SessionKeynotes { get; set; }
        public bool SessionWorkshops { get; set; }
        public bool SessionBreakouts { get; set; }
        public string Track { get; set; } = "";
        public bool AddonDinner { get; set; }
        public bool AddonVip { get; set; }
        public int Attendees { get; set; } = 1;

        // Tab 3: Accommodation
        public string Hotel { get; set; } = "";
        public string RoomType { get; set; } = "";
        public DateTime CheckIn { get; set; } = DateTime.Today;
        public DateTime CheckOut { get; set; } = DateTime.Today;
        public string SpecialRequests { get; set; } = "";
        public bool TransportAirportPickup { get; set; }
        public bool TransportShuttle { get; set; }

        // Tab 4: Payment
        public string PaymentMethod { get; set; } = "";
        public string CardNumber { get; set; } = "";
        public string CardExpiry { get; set; } = "";
        public string CardCvv { get; set; } = "";
        public string BillingAddress { get; set; } = "";
        public string PromoCode { get; set; } = "";

        public double EstimatedTotal { get; set; }
    }
}
