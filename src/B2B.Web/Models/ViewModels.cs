using System.ComponentModel.DataAnnotations;

namespace B2B.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username or Email is required")]
        [Display(Name = "Username or Email")]
        public string UserNameOrEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }
    }

    public class CartItemViewModel
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int AvailableStock { get; set; }
        public decimal Subtotal => Quantity * Price;
    }
}
