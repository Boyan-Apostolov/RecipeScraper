// See https://aka.ms/new-console-template for more information

using System.Net;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using RestSharp;

Console.WriteLine("Hello, RecipeScraper!");

var baseUrl = "https://recepti.gotvach.bg/";
var restClient = new RestClient(baseUrl);

var recipesLinks = await GetPageRecipesLinks(2);

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
        PrintError($"{e.Message}. Problem from recipe: {recipeLink}");
        PrintError($"{e.StackTrace}");
    }
}

;

async Task<HtmlNode> GetDocumentNode(string url)
{
    var response = await restClient.ExecuteAsync(new RestRequest(url),
        Method.Get);
    if (response.StatusCode != HttpStatusCode.OK) throw new InvalidOperationException($"Page {url} not found!");

    var doc = new HtmlDocument();
    doc.LoadHtml(response.Content);

    return doc.DocumentNode;
}

async Task<List<string>> GetPageRecipesLinks(int pages)
{
    var foundLinks = new List<string>();
    for (int i = 1; i <= pages; i++)
    {
        try
        {
            var recipesPage = $"{i}?=6";
            var currentPage = await GetDocumentNode(recipesPage);

            var currentPageRecipeLinks = currentPage.QuerySelectorAll(".rprev>div") //Recipe block
                .Select(recipeBlock => recipeBlock.QuerySelector(".title") // Title, containing the url of the recipe
                    .GetAttributeValue("href", ""))
                .ToList();
            foundLinks.AddRange(currentPageRecipeLinks);
        }
        catch (Exception e)
        {
            PrintError(e.Message);
        }
    }

    return foundLinks;
}

async Task<Recipe> GetRecipeData(string recipeUrl)
{
    var currentPage = await GetDocumentNode(recipeUrl);

    var recipe = new Recipe();

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
            });
        }
    }

    return recipe;
}

void PrintError(string errorMsg)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(errorMsg);
    Console.ForegroundColor = ConsoleColor.White;
}

public class Recipe
{
    public string Title { get; set; }
    public string Url { get; set; }
    public TimeSpan? TotalCookingTime { get; set; }
    public int Portions { get; set; }

    public ICollection<Product> Products { get; set; }

    public string CookingInstructions { get; set; }
}

public class Product
{
    public string Name { get; set; }

    public ProductQuantity ProductQuantity { get; set; }

    public ProductPurpose? ProductPurpose { get; set; }
}

public class ProductQuantity
{
    public string QuantityText { get; set; }
}

public class ProductPurpose
{
    public string Name { get; set; }
}