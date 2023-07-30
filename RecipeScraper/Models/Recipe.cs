namespace RecipeScraper.Models;

public class Recipe
{
    public string Title { get; set; }
    public string Url { get; set; }
    public TimeSpan? TotalCookingTime { get; set; }
    public int Portions { get; set; }

    public ICollection<Product> Products { get; set; }

    public string CookingInstructions { get; set; }
}