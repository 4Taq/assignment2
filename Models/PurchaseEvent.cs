using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;

namespace COMP_2139_Assignment_1.Models;

public class PurchaseEvent 
{
    [ForeignKey("EventId")]
    public int EventId { get; set; }
    public Event Event { get; set; }
    [ForeignKey("PurchaseId")]
    public int PurchaseId { get; set; }
    public Purchase Purchase { get; set; }
}