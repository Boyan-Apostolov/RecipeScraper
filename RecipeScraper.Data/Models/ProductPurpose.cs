using System.ComponentModel.DataAnnotations;

namespace RecipeScraper.Models;

public class ProductPurpose
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<RecipeProduct> RecipeProducts { get; set; }
}