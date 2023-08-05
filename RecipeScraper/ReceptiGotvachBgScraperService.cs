using System.Net;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using RecipeScraper.Models;
using RestSharp;

namespace RecipeScraper;

public class ReceptiGotvachBgScraperService
{
    private const string BaseUrl = "https://recepti.gotvach.bg/";
    private readonly RestClient _restClient;
    private readonly CommonServiceHelper _helper;

    public ReceptiGotvachBgScraperService()
    {
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
        var currentPage = await _helper.GetDocumentNode(recipeUrl);

        var recipe = new Recipe();

        if (recipeUrl == "https://recepti.gotvach.bg/r-258163-Диетична_руска_салата")
        {
            ;
        }

        recipe.Url = recipeUrl;
        recipe.Title = currentPage.QuerySelector("h1").InnerText;

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

        recipe.Products = new List<Product>();
        var productItems = currentPage
            .QuerySelectorAll(".products.new li");
        
        var recipeCategory = currentPage.QuerySelector(".topir.left").InnerText;
        if (!string.IsNullOrWhiteSpace(recipeCategory) && recipeCategory != "ПОДОБНИ РЕЦЕПТИ")
        {
            recipe.RecipeCategory = new RecipeCategory() { Name = recipeCategory };
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

               
                recipe.Products.Add(new Product()
                {
                    Name = productItemName,
                    ProductQuantity = new ProductQuantity() { QuantityText = productItemQuantityText },
                    ProductPurpose = new ProductPurpose() { Name = lastSubItem },
                    // ProductCategory = new ProductCategory() { Name = productCategory }
                });
            }
        }

        return recipe;
    }
}