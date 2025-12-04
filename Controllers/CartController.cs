using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMP_2139_Assignment_1.Models;
using COMP_2139_Assignment_1.Data;
using Microsoft.AspNetCore.Identity;

[Authorize]
public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public CartController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    [HttpPost]
    public async Task<IActionResult> Add(int eventId, int qty = 1)
    {
        var ev = await _context.Events.FindAsync(eventId);
        if (ev == null)
        {
            return Json(new { success = false, message = "Event not found" });
        }


        var soldTickets = await _context.Tickets.CountAsync(t => t.EventId == eventId);
        var availableCapacity = ev.Capacity - soldTickets;

        if (availableCapacity < qty)
        {
            return Json(new { 
                success = false, 
                message = $"Only {availableCapacity} tickets available" 
            });
        }

        // Instead of creating order immediately, redirect to purchase page
        return Json(new { 
            success = true, 
            redirectUrl = Url.Action("Create", "Purchase", new { eventId, quantity = qty })
        });
    }

    [HttpPost]
    public async Task<JsonResult> QuickPurchase(int eventId, int qty = 1)
    {
        var ev = await _context.Events.FindAsync(eventId);
        if (ev == null)
        {
            return Json(new { success = false, message = "Event not found" });
        }

        var soldTickets = await _context.Tickets.CountAsync(t => t.EventId == eventId);
        var availableCapacity = ev.Capacity - soldTickets;

        if (availableCapacity < qty)
        {
            return Json(new { 
                success = false, 
                message = $"Only {availableCapacity} tickets available" 
            });
        }

        var user = await _userManager.GetUserAsync(User);
        var totalPrice = (decimal)(ev.Price.GetValueOrDefault() * qty);

        var order = new Order
        {
            UserId = user.Id,
            EventId = eventId,
            Quantity = qty,
            TotalPrice = totalPrice,
            PurchasedAt = DateTime.UtcNow
        };
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();


        for (int i = 0; i < qty; i++)
        {
            _context.Tickets.Add(new Ticket 
            { 
                EventId = eventId, 
                UserId = user.Id,
                TicketCode = Guid.NewGuid().ToString(),
                PurchasedAt = DateTime.UtcNow
            });
        }
        await _context.SaveChangesAsync();

        return Json(new { 
            success = true, 
            count = qty, 
            total = totalPrice,
            orderId = order.Id
        });
    }


    [HttpGet]
    public IActionResult GetSummary()
    {
        var count = HttpContext.Session.GetInt32("CartCount") ?? 0;
        return Json(new { count });
    }


    public IActionResult Index()
    {
        return View();
    }
}