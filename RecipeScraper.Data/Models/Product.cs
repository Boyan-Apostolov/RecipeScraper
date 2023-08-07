using System.ComponentModel.DataAnnotations;

namespace RecipeScraper.Models;

public class Product
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
}