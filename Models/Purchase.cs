using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Npgsql.Internal.Postgres;

namespace COMP_2139_Assignment_1.Models;

public class Purchase
{
    [Key]
    public int Id { get; set; }
    
    [Required] 
    [DataType(DataType.Date)]
    [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    public DateTime Date { get; set; }
    
    [Required]
    public string Name { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    
    public int? Cost {get; set;}
    
    public List<PurchaseEvent>? PurchaseEvents { get; set; }
}