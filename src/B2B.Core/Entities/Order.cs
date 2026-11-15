using System;
using System.Collections.Generic;

namespace B2B.Core.Entities
{
    public class Order
    {
        public int OrderID { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public int AgentID { get; set; }
        public string OrderStatus { get; set; } = "Pending"; // "Pending", "Processing", "Shipped", "Delivered", "Cancelled"

        // Navigation
        public Agent? Agent { get; set; }
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
