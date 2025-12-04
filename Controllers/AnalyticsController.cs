using COMP_2139_Assignment_1.Data;
using COMP_2139_Assignment_1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Organizer,Admin")]
public class AnalyticsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AnalyticsController(ApplicationDbContext c, UserManager<ApplicationUser> u) => (_context, _userManager) = (c, u);

    public IActionResult Index() => View();

    public async Task<JsonResult> SalesByCategory()
    {
        var userId = _userManager.GetUserId(User);
        var data = await _context.Tickets
            .Include(t => t.Event)
            .Where(t => t.Event!.OrganizerId == userId)
            .GroupBy(t => t.Event!.Category!.Name)
            .Select(g => new { category = g.Key, sales = g.Count() })
            .ToListAsync();
        return Json(data);
    }

    public async Task<JsonResult> RevenueByMonth()
    {
        var userId = _userManager.GetUserId(User);
        var data = await _context.Orders
            .Include(o => o.Event)
            .Where(o => o.Event!.OrganizerId == userId)
            .GroupBy(o => new { o.PurchasedAt.Year, o.PurchasedAt.Month })
            .Select(g => new { month = $"{g.Key.Year}-{g.Key.Month:D2}", revenue = g.Sum(x => x.TotalPrice) })
            .OrderBy(x => x.month)
            .ToListAsync();
        return Json(data);
    }
}


