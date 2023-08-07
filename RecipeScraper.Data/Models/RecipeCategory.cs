using System.ComponentModel.DataAnnotations;

namespace RecipeScraper.Models;

public class RecipeCategory
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<Recipe> Recipes { get; set; }
}