namespace COMP_2139_Assignment_1.Models;

public class Order
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public int? Rating { get; set; } // 1–5 stars
    public DateTime PurchasedAt { get; set; } = DateTime.UtcNow;
}