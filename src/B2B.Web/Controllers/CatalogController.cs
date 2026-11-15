using System.Linq;
using System.Threading.Tasks;
using B2B.Core.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace B2B.Web.Controllers
{
    public class CatalogController : Controller
    {
        private readonly B2BDbContext _context;

        public CatalogController(B2BDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? search, string? filter)
        {
            var query = _context.Items.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(i => i.ItemName.Contains(search) || i.Size.Contains(search));
            }

            if (filter == "in_stock")
            {
                query = query.Where(i => i.StockQuantity > 0);
            }
            else if (filter == "low_stock")
            {
                query = query.Where(i => i.StockQuantity < 200);
            }

            var items = await query.OrderBy(i => i.ItemName).ToListAsync();
            ViewData["Search"] = search;
            ViewData["Filter"] = filter;

            return View(items);
        }
    }
}
