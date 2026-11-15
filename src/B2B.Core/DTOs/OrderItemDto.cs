namespace B2B.Core.DTOs
{
    public class OrderItemDto
    {
        public int ItemID { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int? OrderID { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
