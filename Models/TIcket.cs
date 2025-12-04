namespace COMP_2139_Assignment_1.Models;

public class Ticket
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public string TicketCode { get; set; } = Guid.NewGuid().ToString();
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
}
