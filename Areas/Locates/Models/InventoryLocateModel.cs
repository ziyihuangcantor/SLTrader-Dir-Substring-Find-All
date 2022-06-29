namespace SLTrader.Models
{
    public class InventoryLocateModel
    {
        public int      IssueId { get; set; }
        public string   Source { get; set; }
        public decimal  Available { get; set; }
    }
}