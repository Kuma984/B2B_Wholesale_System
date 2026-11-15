using System.Linq;
using System.Threading.Tasks;
using B2B.Core.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace B2B.Web.Controllers
{
    public class OrdersController : Controller
    {
        private readonly B2BDbContext _context;

        public OrdersController(B2BDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var agentId = HttpContext.Session.GetInt32("AgentID");
            if (agentId == null)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Orders") });
            }

            var orders = await _context.Orders
                .Where(o => o.AgentID == agentId.Value)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Invoice(int id)
        {
            var agentId = HttpContext.Session.GetInt32("AgentID");
            if (agentId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders
                .Include(o => o.Agent)
                .Include(o => o.OrderDetails)
                .ThenInclude(d => d.Item)
                .FirstOrDefaultAsync(o => o.OrderID == id);

            if (order == null)
            {
                return NotFound();
            }

            // Ensure agent can only view their own invoice
            if (order.AgentID != agentId.Value)
            {
                return Forbid();
            }

            return View(order);
        }
    }
}
