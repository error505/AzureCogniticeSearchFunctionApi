using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using AzureCogniticeSearchApi.Models;

namespace AzureCogniticeSearchApi.Functions.Suggest
{
	public class SearchSuggestionDto
    {
        public Dictionary<string, List<SearchSuggestion<Project>>> ResultList { get; set; }
    }

    public static class Suggest
    {
        [FunctionName("SuggestProjects")]
        public static async Task<IActionResult> RunSuggestProjects(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ClaimsPrincipal claimsPrincipal,
            ILogger log)
        {
            if (!claimsPrincipal.Identity.IsAuthenticated)
            {
                return new UnauthorizedResult();
            }

            Uri serviceEndpoint = new Uri($"https://{Environment.GetEnvironmentVariable("SearchServiceName")}.search.windows.net/");
            AzureKeyCredential credential = new AzureKeyCredential(Environment.GetEnvironmentVariable("SearchApiKey")!);
            SearchIndexClient adminClient = new SearchIndexClient(serviceEndpoint, credential);

            SearchClient ingesterClient = adminClient.GetSearchClient(Environment.GetEnvironmentVariable("IndexName"));
            string searchText = req.Query["SearchText"];

            var options = new SuggestOptions()
            {
                UseFuzzyMatching = true,
                Size = 5
            };

            var result = new SearchSuggestionDto();
            var suggesterResponse = await ingesterClient.SuggestAsync<Project>(searchText, Environment.GetEnvironmentVariable("SuggesterName"), options);
            var response = new Dictionary<string, List<SearchSuggestion<Project>>>();
            response["suggestions"] = suggesterResponse.Value.Results.ToList();
            result.ResultList = response;
            return new OkObjectResult(result);
        }
    }
}
