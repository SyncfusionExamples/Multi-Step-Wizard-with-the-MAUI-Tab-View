namespace MultiStepForm
{
    public static class PickerData
    {
        // Exposes a read-only list of partner hotel names used to populate HotelPicker
        public static readonly IReadOnlyList<string> Hotels = new[]
        {
            "Hotel Aurora", "Cityscape Suites", "Grand Meridian", "Harbor View"
        };

        // Room types used to populate RoomTypePicker
        public static readonly IReadOnlyList<string> RoomTypes = new[]
        {
            "Single", "Double", "Suite"
        };

        // Conference tracks used to populate TrackPicker
        public static readonly IReadOnlyList<string> Tracks = new[]
        {
            "Tech", "Business", "Design", "Product"
        };

        // Payment methods used to populate PaymentMethodPicker
        public static readonly IReadOnlyList<string> PaymentMethods = new[]
        {
            "Credit Card", "Debit Card", "UPI", "PayPal"
        };
    }
}
