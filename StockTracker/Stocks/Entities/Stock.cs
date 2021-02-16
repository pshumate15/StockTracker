using StockTracker.Common;

namespace StockTracker.Stocks.Entities
{
    public class Stock : AuditableEntity
    {
        public int StockID { get; set; }

        public string Symbol { get; set; }

        public string Name { get; set; }

        public string Industry { get; set; }
    }
}