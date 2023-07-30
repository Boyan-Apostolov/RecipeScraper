using System.Net;
using HtmlAgilityPack;
using RestSharp;

namespace RecipeScraper;

public class CommonServiceHelper
{
    private readonly RestClient _restClient;

    public CommonServiceHelper(RestClient restClient)
    {
        _restClient = restClient;
    }

    public async Task<HtmlNode> GetDocumentNode(string url)
    {
        var response = await _restClient.ExecuteAsync(new RestRequest(url),
            Method.Get);
        if (response.StatusCode != HttpStatusCode.OK) throw new InvalidOperationException($"Page {url} not found!");

        var doc = new HtmlDocument();
        doc.LoadHtml(response.Content);

        return doc.DocumentNode;
    }

    public void PrintError(string errorMsg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(errorMsg);
        Console.ForegroundColor = ConsoleColor.White;
    }
}