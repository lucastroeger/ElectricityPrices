namespace ElectricityPricesApp.Models
{
    public class DateRangeModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public string BiddingZone { get; set; }
        public string SelectedLocation { get; set; }
    }
}