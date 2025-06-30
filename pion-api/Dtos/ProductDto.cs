using System.Text.Json;

namespace pion_api.Dtos;

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Tags { get; set; } = "[]";
    public IFormFile? File { get; set; }

    public List<string> GetTagList()
    {
        return JsonSerializer.Deserialize<List<string>>(Tags) ?? new();
    }
}