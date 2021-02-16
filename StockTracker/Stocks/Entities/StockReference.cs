using StockTracker.Reddit.Entities;

namespace StockTracker.Stocks.Entities
{
    public class StockReference
    {
        public int StockReferenceID { get; set; }

        public Stock Stock { get; set; }

        public Comment Comment { get; set; }
    }
}