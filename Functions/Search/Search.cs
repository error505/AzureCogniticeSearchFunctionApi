using System;
using System.Collections.Generic;
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

namespace AzureCogniticeSearchApi.Functions.Search
{
	public class SearchDto
	{
		public List<SearchResult<Project>> ResultList { get; set; }
	}
	public static class Search
	{
		[FunctionName("SearchProjects")]
		public static async Task<IActionResult> RunSearchMyTasks(
			[HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
			HttpRequest req,
			ClaimsPrincipal claimsPrincipal,
			ILogger log)
		{
			if (!claimsPrincipal.Identity.IsAuthenticated)
			{
				return new UnauthorizedResult();
			}

			string searchText = req.Query["SearchText"];
			var searchIndexClient = GetSearchIndexClient();

			if (searchIndexClient == null) return GetResponseObject(new List<SearchResult<Project>>());

			var myTaskReponse = await GetReponse(searchIndexClient, searchText);

			return GetResponseObject(myTaskReponse);
		}

		private static IActionResult GetResponseObject(List<SearchResult<Project>> searchResponse)
		   => new OkObjectResult(new SearchDto()
		   {
			   ResultList = searchResponse
		   });

		private static async Task<List<SearchResult<Project>>> GetReponse(SearchIndexClient searchIndexClient, string searchText)
		{
			SearchResults<Project> searchResult = await GetSearchResult(searchIndexClient, searchText);

			if (searchResult == null) return new List<SearchResult<Project>>();

			var searchResponse = new List<SearchResult<Project>>();

			await foreach (var result in searchResult.GetResultsAsync().AsPages(default, int.MaxValue))
			{
				if (result?.Values == null) continue;

				for (int i = 0; i < result.Values.Count; i++)
				{
					searchResponse.Add(result.Values[i]);
				}
			}

			return searchResponse;
		}

		private static async Task<SearchResults<Project>> GetSearchResult(SearchIndexClient searchIndexClient, string searchText)
		{
			var searchClient = searchIndexClient.GetSearchClient(Environment.GetEnvironmentVariable("IndexName"));

			SearchResults<Project> searchResult = await searchClient.SearchAsync<Project>(searchText, new SearchOptions() { IncludeTotalCount = true });
			return searchResult;
		}

		private static SearchIndexClient GetSearchIndexClient()
		{
			try
			{
				var serviceEndpoint = new Uri($"https://{Environment.GetEnvironmentVariable("SearchServiceName")}.search.windows.net/");
				var credential = new AzureKeyCredential(Environment.GetEnvironmentVariable("SearchApiKey")!);

				var searchIndexClient = new SearchIndexClient(serviceEndpoint, credential);
				return searchIndexClient;
			}
			catch
			{
				return null;
			}
		}
	}
}
