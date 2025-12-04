using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMP_2139_Assignment_1.Data;
using COMP_2139_Assignment_1.Models;

public class EventController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public EventController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }


    public async Task<IActionResult> Index()
    {
        var events = await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Organizer)
            .OrderBy(e => e.StartDate)
            .ToListAsync();

        return View(events);
    }


    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var eventItem = await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Organizer)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (eventItem == null)
        {
            return NotFound();
        }


        var soldTickets = await _context.Tickets.CountAsync(t => t.EventId == id);
        ViewBag.AvailableTickets = eventItem.Capacity - soldTickets;

        return View(eventItem);
    }


    public async Task<IActionResult> Search(string q)
    {
        var events = string.IsNullOrEmpty(q)
            ? await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Organizer)
                .ToListAsync()
            : await _context.Events
                .Include(e => e.Category)
                .Include(e => e.Organizer)
                .Where(e => e.Name.Contains(q) || 
                           e.Category!.Name.Contains(q))
                .ToListAsync();

        return PartialView("_EventCard", events);
    }


    [Authorize(Roles = "Organizer,Admin")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Event evnt)
    {
        if (evnt.EndDate < evnt.StartDate)
            ModelState.AddModelError("EndDate", "End date must be after start date.");

        if (evnt.StartDate < DateTime.Today)
            ModelState.AddModelError("StartDate", "Start date cannot be in the past.");

        if (ModelState.IsValid)
        {
            
            evnt.StartDate = DateTime.SpecifyKind(evnt.StartDate, DateTimeKind.Utc);
            evnt.EndDate   = DateTime.SpecifyKind(evnt.EndDate,   DateTimeKind.Utc);

            evnt.OrganizerId = _userManager.GetUserId(User);
            _context.Events.Add(evnt);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Event created successfully!";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(evnt);
    }


    [Authorize(Roles = "Organizer,Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var eventItem = await _context.Events.FindAsync(id);
        
        if (eventItem == null)
        {
            return NotFound();
        }

        // Check if user is the organizer or admin
        var userId = _userManager.GetUserId(User);
        if (eventItem.OrganizerId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(eventItem);
    }


    [HttpPost]
    [Authorize(Roles = "Organizer,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Event evnt)
    {
        if (id != evnt.Id)
        {
            return NotFound();
        }
        
        evnt.StartDate =  DateTime.SpecifyKind(evnt.StartDate, DateTimeKind.Utc);
        evnt.EndDate = DateTime.SpecifyKind(evnt.EndDate, DateTimeKind.Utc);
        
        if (evnt.EndDate < evnt.StartDate)
        {
            ModelState.AddModelError("EndDate", "End date must be after start date.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                var existingEvent = await _context.Events.AsNoTracking()
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (existingEvent == null)
                {
                    return NotFound();
                }
                
                if (existingEvent.OrganizerId != userId && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }
                
                evnt.OrganizerId = existingEvent.OrganizerId;

                _context.Update(evnt);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Event updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await EventExists(evnt.Id))
                {
                    return NotFound();
                }
                throw;
            }
        }

        ViewBag.Categories = await _context.Categories.ToListAsync();
        return View(evnt);
    }

    [Authorize(Roles = "Organizer,Admin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var eventItem = await _context.Events
            .Include(e => e.Category)
            .Include(e => e.Organizer)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (eventItem == null)
        {
            return NotFound();
        }
        
        var userId = _userManager.GetUserId(User);
        if (eventItem.OrganizerId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return View(eventItem);
    }


    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = "Organizer,Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var eventItem = await _context.Events.FindAsync(id);
        
        if (eventItem == null)
        {
            return NotFound();
        }
        
        var userId = _userManager.GetUserId(User);
        if (eventItem.OrganizerId != userId && !User.IsInRole("Admin"))
        {
            return Forbid();
        }


        var hasTickets = await _context.Tickets.AnyAsync(t => t.EventId == id);
        if (hasTickets)
        {
            TempData["Error"] = "Cannot delete event with sold tickets.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        _context.Events.Remove(eventItem);
        await _context.SaveChangesAsync();
        
        TempData["Success"] = "Event deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> EventExists(int id)
    {
        return await _context.Events.AnyAsync(e => e.Id == id);
    }
}