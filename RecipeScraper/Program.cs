using System.Text.Json;
using RecipeScraper;

var receptiGotvachScraper = new ReceptiGotvachBgScraperService();
var firstPageRecipes = await receptiGotvachScraper.GetRecipes(1, 1);
var jsonToSave = JsonSerializer.Serialize(firstPageRecipes);

await File.WriteAllTextAsync("data.json", jsonToSave);








