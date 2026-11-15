using System.Collections.Generic;

namespace B2B.Core.Entities
{
    public class Item
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        // Navigation
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
