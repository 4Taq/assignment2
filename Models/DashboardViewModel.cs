namespace COMP_2139_Assignment_1.Models;

public class DashboardViewModel
{
    public List<Ticket> MyTickets { get; set; } = new();
    public List<Order> PurchaseHistory { get; set; } = new();
    public List<Event> MyEvents { get; set; } = new();
    public decimal TotalRevenue { get; set; }
    public ApplicationUser User { get; set; } = null!;
}