using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using B2B.Core.Data;
using B2B.Core.DTOs;
using B2B.Core.Services;
using B2B.Web.Extensions;
using B2B.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace B2B.Web.Controllers
{
    public class CartController : Controller
    {
        private readonly B2BDbContext _context;
        private readonly IOrderService _orderService;
        private const string CartSessionKey = "B2B_Cart";

        public CartController(B2BDbContext context, IOrderService orderService)
        {
            _context = context;
            _orderService = orderService;
        }

        private List<CartItemViewModel> GetCart()
        {
            return HttpContext.Session.GetObjectFromJson<List<CartItemViewModel>>(CartSessionKey) 
                   ?? new List<CartItemViewModel>();
        }

        private void SaveCart(List<CartItemViewModel> cart)
        {
            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
        }

        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int itemId, int quantity = 1)
        {
            if (quantity <= 0) quantity = 1;

            var item = await _context.Items.FindAsync(itemId);
            if (item == null)
            {
                TempData["ErrorMessage"] = "Item not found.";
                return RedirectToAction("Index", "Catalog");
            }

            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.ItemID == itemId);

            if (existing != null)
            {
                existing.Quantity += quantity;
                existing.AvailableStock = item.StockQuantity;
            }
            else
            {
                cart.Add(new CartItemViewModel
                {
                    ItemID = item.ItemID,
                    ItemName = item.ItemName,
                    Size = item.Size,
                    Price = item.Price,
                    Quantity = quantity,
                    AvailableStock = item.StockQuantity
                });
            }

            SaveCart(cart);
            TempData["SuccessMessage"] = $"Added '{item.ItemName}' to your order.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int itemId, int quantity)
        {
            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.ItemID == itemId);
            if (existing != null)
            {
                if (quantity <= 0)
                {
                    cart.Remove(existing);
                }
                else
                {
                    existing.Quantity = quantity;
                }
                SaveCart(cart);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remove(int itemId)
        {
            var cart = GetCart();
            cart.RemoveAll(c => c.ItemID == itemId);
            SaveCart(cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            // Verify agent logged in
            var agentId = HttpContext.Session.GetInt32("AgentID");
            if (agentId == null)
            {
                TempData["ErrorMessage"] = "Please log in with your registered Agent account to submit orders.";
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Cart") });
            }

            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty. Please add items to place an order.";
                return RedirectToAction("Index", "Catalog");
            }

            // Convert to DTO
            var orderItems = cart.Select(c => new OrderItemDto
            {
                ItemID = c.ItemID,
                Quantity = c.Quantity
            }).ToList();

            // EXECUTE ATOMIC TRANSACTION WITH STOCK DEDUCTION
            var result = await _orderService.PlaceOrderAsync(agentId.Value, orderItems);

            if (!result.Success)
            {
                TempData["ErrorMessage"] = $"Order failed: {result.Message}";
                return RedirectToAction("Index");
            }

            // Clear cart upon successful transactional commit
            HttpContext.Session.Remove(CartSessionKey);
            TempData["SuccessMessage"] = $"Order #{result.OrderID} has been placed successfully! Total: {result.TotalAmount:N0} VND.";

            return RedirectToAction("Invoice", "Orders", new { id = result.OrderID });
        }
    }
}
