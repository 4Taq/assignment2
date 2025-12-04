using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMP_2139_Assignment_1.Data;
using COMP_2139_Assignment_1.Models;

namespace COMP_2139_Assignment_1.Controllers;

[Authorize]
public class PurchaseController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    
    public PurchaseController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    [HttpGet]
    public async Task<IActionResult> Create(int eventId)
    {
        var eventItem = await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Organizer)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (eventItem == null)
        {
            return NotFound();
        }


        var soldTickets = await _context.Tickets.CountAsync(t => t.EventId == eventId);
        var availableCapacity = eventItem.Capacity - soldTickets;

        if (availableCapacity <= 0)
        {
            TempData["Error"] = "This event is sold out.";
            return RedirectToAction("Index", "Event");
        }

        ViewBag.Event = eventItem;
        ViewBag.AvailableCapacity = availableCapacity;
        
        return View();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int eventId, int quantity)
    {
        var eventItem = await _context.Events.FindAsync(eventId);
        
        if (eventItem == null)
        {
            return NotFound();
        }


        if (quantity <= 0)
        {
            ModelState.AddModelError("", "Quantity must be at least 1.");
            return await Create(eventId);
        }


        var soldTickets = await _context.Tickets.CountAsync(t => t.EventId == eventId);
        var availableCapacity = eventItem.Capacity - soldTickets;

        if (quantity > availableCapacity)
        {
            ModelState.AddModelError("", $"Only {availableCapacity} tickets available.");
            return await Create(eventId);
        }

        var user = await _userManager.GetUserAsync(User);
        var totalPrice = (decimal)(eventItem.Price.GetValueOrDefault() * quantity);


        var order = new Order
        {
            UserId = user.Id,
            EventId = eventId,
            Quantity = quantity,
            TotalPrice = totalPrice,
            PurchasedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Create individual tickets
        for (int i = 0; i < quantity; i++)
        {
            var ticket = new Ticket
            {
                EventId = eventId,
                UserId = user.Id,
                TicketCode = Guid.NewGuid().ToString(),
                PurchasedAt = DateTime.UtcNow
            };
            _context.Tickets.Add(ticket);
        }

        await _context.SaveChangesAsync();

        TempData["Success"] = $"Successfully purchased {quantity} ticket(s)!";
        return RedirectToAction(nameof(Confirmation), new { id = order.Id });
    }


    [HttpGet]
    public async Task<IActionResult> Confirmation(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Event)
            .ThenInclude(e => e.Category)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        // Ensure user can only view their own orders
        var userId = _userManager.GetUserId(User);
        if (order.UserId != userId)
        {
            return Forbid();
        }


        var tickets = await _context.Tickets
            .Include(t => t.Event)
            .Where(t => t.UserId == userId && 
                       t.EventId == order.EventId && 
                       t.PurchasedAt >= order.PurchasedAt.AddSeconds(-5) &&
                       t.PurchasedAt <= order.PurchasedAt.AddSeconds(5))
            .ToListAsync();

        ViewBag.Tickets = tickets;

        return View(order);
    }
}