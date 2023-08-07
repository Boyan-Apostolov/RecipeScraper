using System.Net;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using RecipeScraper.Data;
using RecipeScraper.Models;
using RestSharp;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace RecipeScraper;

public class ReceptiGotvachBgScraperService
{
    private const string BaseUrl = "https://recepti.gotvach.bg";
    private readonly RestClient _restClient;
    private readonly CommonServiceHelper _helper;
    private readonly RecipesDbContext _recipesDbContext;

    public ReceptiGotvachBgScraperService(RecipesDbContext recipesDbContext)
    {
        _recipesDbContext = recipesDbContext;
        _restClient = new RestClient(BaseUrl);
        _helper = new CommonServiceHelper(_restClient);
    }

    public async Task<ICollection<Recipe>> GetRecipes(int startPage, int endPage)
    {
        var recipesLinks = await GetPageRecipesLinks(startPage, endPage);

        var parsedRecipes = new List<Recipe>();
        foreach (var recipeLink in recipesLinks)
        {
            try
            {
                var recipe = await GetRecipeData(recipeLink);
                parsedRecipes.Add(recipe);
            }
            catch (Exception e)
            {
                _helper.PrintError($"{e.Message}. Problem from recipe: {recipeLink}");
                _helper.PrintError($"{e.StackTrace}");
            }
        }

        await _recipesDbContext.SaveChangesAsync();
        return parsedRecipes;
    }

    async Task<List<string>> GetPageRecipesLinks(int startPage, int endPage)
    {
        var foundLinks = new List<string>();
        for (int i = startPage; i <= endPage; i++)
        {
            try
            {
                var recipesPage = $"{i}?=6";
                var currentPage = await _helper.GetDocumentNode(recipesPage);

                var currentPageRecipeLinks = currentPage.QuerySelectorAll(".rprev>div") //Recipe block
                    .Select(recipeBlock => recipeBlock
                        .QuerySelector(".title") // Title, containing the url of the recipe
                        .GetAttributeValue("href", ""))
                    .ToList();
                foundLinks.AddRange(currentPageRecipeLinks);
            }
            catch (Exception e)
            {
                _helper.PrintError(e.Message);
            }
        }

        return foundLinks;
    }

    async Task<Recipe> GetRecipeData(string recipeUrl)
    {
        var alreadySavedRecipe = await _recipesDbContext
            .Recipes
            .FirstOrDefaultAsync(r => r.Url == recipeUrl);
        if (alreadySavedRecipe != null) return alreadySavedRecipe;

        var currentPage = await _helper.GetDocumentNode(recipeUrl);

        var recipe = new Recipe();

        recipe.Url = recipeUrl;
        recipe.Title = currentPage.QuerySelector("h1").InnerText;

        var foundImageUrl = currentPage.QuerySelector("#gall img").GetAttributeValue("src", "");
        recipe.ImageUrl = !string.IsNullOrWhiteSpace(foundImageUrl) && !foundImageUrl.Contains("addnew")
            ? $"{BaseUrl}{foundImageUrl}"
            : "";

        var totalTimeText = currentPage //Format : "125 mins."
            .QuerySelector(".icb-tot")?
            .ChildNodes[1]
            .InnerText
            .Split(" ")[0];
        recipe.TotalCookingTime = string.IsNullOrWhiteSpace(totalTimeText)
            ? null
            : TimeSpan.FromMinutes(int.Parse(totalTimeText));

        var portionsText = currentPage //Format : "1"
            .QuerySelector(".icb-fak")?
            .ChildNodes[1]
            .InnerText;
        recipe.Portions = string.IsNullOrWhiteSpace(portionsText)
            ? 1
            : int.Parse(portionsText);

        recipe.CookingInstructions = currentPage.QuerySelector(".text").InnerText
            .Replace("Начин на приготвяне", "");

        recipe.RecipeProducts = new List<RecipeProduct>();
        var productItems = currentPage
            .QuerySelectorAll(".products.new li");

        var recipeCategory = currentPage.QuerySelector(".topir.left").InnerText;
        if (!string.IsNullOrWhiteSpace(recipeCategory) && recipeCategory != "ПОДОБНИ РЕЦЕПТИ")
        {
            var foundRecipeCategory = await _recipesDbContext
                .RecipeCategories
                .FirstOrDefaultAsync(rc => rc.Name == recipeCategory);

            recipe.RecipeCategory = foundRecipeCategory != null
                ? foundRecipeCategory
                : new RecipeCategory() { Name = recipeCategory };
        }

        var lastSubItem = string.Empty;
        foreach (var productItem in productItems)
        {
            var itemsPurpose = productItem.QuerySelector(".sub");
            if (itemsPurpose != null)
            {
                //The recipe has multiple sub-items, the following items until the next .sub are used for the current sub-item
                lastSubItem = itemsPurpose.InnerText;
                continue;
            }
            else
            {
                var productItemName = productItem.QuerySelector("b").InnerText;

                var productItemHasQuantityText = productItem
                    .ChildNodes;

                var productItemQuantityText = productItemHasQuantityText.Count == 1
                    ? ""
                    : productItemHasQuantityText[1].InnerText
                        .Split(" - ")[1];

                var foundProductQuantity = await _recipesDbContext
                    .ProductQuantities
                    .FirstOrDefaultAsync(pq => pq.QuantityText == productItemQuantityText);

                var foundProductPurpose = await _recipesDbContext
                    .ProductPurposes
                    .FirstOrDefaultAsync(pp => pp.Name == lastSubItem);

                var foundProduct = await _recipesDbContext
                    .Products
                    .FirstOrDefaultAsync(p => p.Name == productItemName);

                var usableProduct = foundProduct != null
                    ? foundProduct
                    : new Product() { Name = productItemName };

                recipe.RecipeProducts.Add(new RecipeProduct()
                {
                    Product = usableProduct,
                    ProductQuantity = foundProductQuantity != null
                        ? foundProductQuantity
                        : new ProductQuantity() { QuantityText = productItemQuantityText },
                    ProductPurpose = string.IsNullOrWhiteSpace(lastSubItem) || foundProductPurpose == null
                        ? new ProductPurpose() { Name = lastSubItem }
                        : foundProductPurpose,
                });
            }
        }

        _recipesDbContext.Recipes.Add(recipe);
        return recipe;
    }
}