namespace COMP_2139_Assignment_1.Models;

public class Category
{
    public int Id {get; set;}
    public string Name {get; set;}
    public List<Event>? Events { get; set; }
}