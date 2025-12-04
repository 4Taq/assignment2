
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using COMP_2139_Assignment_1.Data;
using COMP_2139_Assignment_1.Models;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        var today = DateTime.UtcNow.Date;
        var model = new DashboardViewModel
        {
            User = user,
            MyTickets = await _context.Tickets
                .Include(t => t.Event)
                .Where(t => t.UserId == user.Id && 
                            t.Event.StartDate >= today) 
                .ToListAsync(),

            PurchaseHistory = await _context.Orders
                .Include(o => o.Event)
                .Where(o => o.UserId == user.Id)
                .ToListAsync(),

            MyEvents = User.IsInRole("Organizer") || User.IsInRole("Admin")
                ? await _context.Events.Where(e => e.OrganizerId == user.Id).ToListAsync()
                : new List<Event>(),

            TotalRevenue = User.IsInRole("Organizer") || User.IsInRole("Admin")
                ? await _context.Orders
                    .Where(o => _context.Events.Any(e => e.Id == o.EventId && e.OrganizerId == user.Id))
                    .SumAsync(o => o.TotalPrice)
                : 0
        };

        return View(model);
    }


    public string GenerateQrCodeBase64(string text)
    {
        var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeData);
        byte[] byteGraphic = qrCode.GetGraphic(10);
        return Convert.ToBase64String(byteGraphic);
    }
}

