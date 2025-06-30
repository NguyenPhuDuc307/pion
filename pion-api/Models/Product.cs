namespace pion_api.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new List<string>();
    public string ImageUrl { get; set; } = string.Empty;
}