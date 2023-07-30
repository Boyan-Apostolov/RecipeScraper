// See https://aka.ms/new-console-template for more information

using System.Net;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using RestSharp;

Console.WriteLine("Hello, World!");

var baseUrl = "https://recepti.gotvach.bg/";
var restClient = new RestClient(baseUrl);

var recipes = await GetPageRecipesLinks(2);
Console.WriteLine(string.Join(Environment.NewLine, recipes));

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
        var recipesPage = $"{i}?=6";
        var currentPage = await GetDocumentNode(recipesPage);
        
        var currentPageRecipeLinks = currentPage.QuerySelectorAll(".rprev>div") //Recipe block
            .Select(recipeBlock => recipeBlock.QuerySelector(".title") // Title, containing the url of the recipe
                .GetAttributeValue("href", ""))
            .ToList();
        foundLinks.AddRange(currentPageRecipeLinks);
    }

    return foundLinks;
}

async Task<Recipe> GetRecipeData(string recipeUrl)
{
    var currentPage = await GetDocumentNode(recipeUrl);

    var recipe = new Recipe();

    recipe.Url = recipeUrl;
    recipe.Title = currentPage.QuerySelector("h1").InnerText;
    
    return recipe;
}

public class Recipe
{
    public string Title { get; set; }

    public string Url { get; set; }
}