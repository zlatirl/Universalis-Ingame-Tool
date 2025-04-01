using System;

namespace MarketAnalysisPlugin
{
    // Simple class to represent market listings from Universalis
    public class MarketListing
    {
        public int ItemId { get; set; }
        public int PricePerUnit { get; set; }
        public int Quantity { get; set; }
        public string WorldName { get; set; } = string.Empty;
        public DateTime LastReviewTime { get; set; }
        public string RetainerName { get; set; } = string.Empty;
        public bool IsHQ { get; set; }
        public bool IsHistory { get; set; }

        // Helper for displaying formatted price
        public string FormattedPrice => PricePerUnit.ToString("N0");

        // Total price calculation
        public int TotalPrice => PricePerUnit * Quantity;
    }
}
