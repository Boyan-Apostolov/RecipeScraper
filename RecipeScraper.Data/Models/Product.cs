namespace RecipeScraper.Models;

public class Product
{
    public string Name { get; set; }

    public ProductQuantity ProductQuantity { get; set; }

    public ProductPurpose? ProductPurpose { get; set; }

    public RecipeCategory RecipeCategory { get; set; }
}