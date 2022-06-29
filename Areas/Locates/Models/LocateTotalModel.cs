namespace SLTrader.Models
{
    public class LocateTotalModel
    {
        public int IssueId { get; set; }
        public string ClientId { get; set; }
        public decimal Requested { get; set; }
        public decimal Located { get; set; }
    }
}