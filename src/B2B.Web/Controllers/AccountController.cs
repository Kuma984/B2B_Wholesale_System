using System.Threading.Tasks;
using B2B.Core.Data;
using B2B.Core.Services;
using B2B.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace B2B.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        private readonly B2BDbContext _context;

        public AccountController(IAuthService authService, B2BDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { UserNameOrEmail = "agent_hanoi01", Password = "Agent@123" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var (success, message, user) = await _authService.AuthenticateAsync(
                model.UserNameOrEmail,
                model.Password,
                "Agent"); // Must be Agent

            if (!success || user == null)
            {
                model.ErrorMessage = message;
                return View(model);
            }

            // Look up associated Agent record
            var agent = await _context.Agents.FirstOrDefaultAsync(a => a.AgentID == user.UserID)
                     ?? await _context.Agents.FirstOrDefaultAsync(); // fallback for seeded demo

            int agentId = agent?.AgentID ?? user.UserID;
            string agentName = agent?.AgentName ?? user.UserName;

            // Store authentication in session
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetInt32("AgentID", agentId);
            HttpContext.Session.SetString("UserName", user.UserName);
            HttpContext.Session.SetString("AgentName", agentName);
            HttpContext.Session.SetString("UserRole", user.Role);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Catalog");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}
