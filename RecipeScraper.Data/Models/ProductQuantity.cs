using System.ComponentModel.DataAnnotations;

namespace RecipeScraper.Models;

public class ProductQuantity
{
    [Key]
    public int Id { get; set; }
    public string QuantityText { get; set; }

    public ICollection<RecipeProduct> RecipeProduct { get; set; }
}