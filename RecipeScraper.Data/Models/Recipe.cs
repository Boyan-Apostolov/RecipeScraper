using System.ComponentModel.DataAnnotations;

namespace RecipeScraper.Models;

public class Recipe
{
    [Key]
    public int Id { get; set; }
    public string Title { get; set; }
    public string Url { get; set; }

    public string ImageUrl { get; set; }
    public TimeSpan? TotalCookingTime { get; set; }
    public int Portions { get; set; }

    public ICollection<RecipeProduct> RecipeProducts { get; set; }

    public string CookingInstructions { get; set; }

    public RecipeCategory? RecipeCategory { get; set; }
}