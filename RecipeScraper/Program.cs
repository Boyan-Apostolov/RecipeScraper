using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RecipeScraper;
using RecipeScraper.Data;

var dbContext = new RecipesDbContext();
dbContext.Database.EnsureCreated();

var receptiGotvachScraper = new ReceptiGotvachBgScraperService(dbContext);

var firstPageRecipes = await receptiGotvachScraper.GetRecipes(1, 1);









