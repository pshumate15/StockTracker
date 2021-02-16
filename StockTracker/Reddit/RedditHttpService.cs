using Microsoft.Extensions.Logging;
using StockTracker.Reddit.Entities;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StockTracker.Reddit
{
	/// <inheritdoc cref="IRedditHttpService"/>
	public class RedditHttpService : IRedditHttpService
	{
		private readonly HttpClient _httpClient;
		private readonly RateLimiter _rateLimiter;
		private readonly ILogger<RedditHttpService> _logger;

		public RedditHttpService(HttpClient httpClient, RateLimiter rateLimiter, ILogger<RedditHttpService> logger)
		{
			_httpClient = httpClient;
			_httpClient.DefaultRequestHeaders.Add("User-Agent", RedditConstants.UserAgent);
			_httpClient.BaseAddress = new Uri("https://oauth.reddit.com");
			_rateLimiter = rateLimiter;
			_logger = logger;
		}

		/// <inheritdoc/>
		public async Task<RedditJson<T>> GetRedditJsonAsync<T>(string uri)
		{
			// Limit number of API requests per minute
			// If the rate limiter buffer is full, try again later
			while (await _rateLimiter.TryAddEventAsync() == false)
			{
				await Task.Delay(200);
			}

			string contentString = "";

			try
			{
				using (var httpResponse = await _httpClient.GetAsync(uri))
				{
					httpResponse.EnsureSuccessStatusCode();
					contentString = await httpResponse.Content.ReadAsStringAsync();
				}
			}
			catch (HttpRequestException ex)
			{
				_logger.LogError(ex, "Exception occurred when trying to make a request to {Uri}", uri);
				throw;
			}

			contentString = CorrectJsonFormat(contentString);

			return new RedditJson<T>
			{
				Listing = JsonSerializer.Deserialize<Thing<Listing<T>>[]>(contentString)
			};
		}

		private string CorrectJsonFormat(string contentString)
		{
			var sb = new StringBuilder(contentString);

			// Handle case when response is a single object instead of an array
			// Convert to a one element array
			if (sb[0] == '{')
			{
				sb.Insert(0, '[');
				sb.Insert(sb.Length, ']');
			}

			// Fix instances where the replies object should be null rather than an empty string
			sb.Replace("\"replies\": \"\"", "\"replies\": null");
			return sb.ToString();
		}
	}
}
