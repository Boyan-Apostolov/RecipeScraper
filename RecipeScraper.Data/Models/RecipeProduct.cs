using System.ComponentModel.DataAnnotations;

namespace RecipeScraper.Models;

public class RecipeProduct
{
    [Key]
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }
    
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; }

    public ProductQuantity ProductQuantity { get; set; }
    public ProductPurpose? ProductPurpose { get; set; }
}