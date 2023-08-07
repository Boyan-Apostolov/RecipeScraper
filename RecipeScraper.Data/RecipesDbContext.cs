using Microsoft.EntityFrameworkCore;
using RecipeScraper.Models;

namespace RecipeScraper.Data;

public class RecipesDbContext : DbContext
{
    public RecipesDbContext(DbContextOptions options) : base(options)
    {
    }

    public RecipesDbContext()
    {
    }

    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<RecipeCategory> RecipeCategories { get; set; }
    public DbSet<Product> Products { get; set; }

    public DbSet<RecipeProduct> ProductRecipes { get; set; }
    public DbSet<ProductPurpose> ProductPurposes { get; set; }
    public DbSet<ProductQuantity> ProductQuantities { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            "Data Source=localhost,1433;Database=recipe-manager;Encrypt=false;Integrated Security=false;User ID=sa;Password=DulgaITrudnaParola!");
        base.OnConfiguring(optionsBuilder);
    }
}