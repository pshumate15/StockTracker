using StockTracker.Reddit.Entities;
using System.Threading.Tasks;

namespace StockTracker.Reddit
{
	/// <summary>
	/// Provides functionality to make requests to Reddit and parse responses.
	/// </summary>
	public interface IRedditHttpService
	{
		/// <summary>
		/// Gets a RedditJson object that represents the response from a request to the given URI.
		/// </summary>
		/// <typeparam name="T">The Reddit entity being requested (e.g. comment or post).</typeparam>
		/// <param name="uri">The Reddit URI at which to get the resource(s).</param>
		/// <returns>A RedditJson representation of the requested resource.</returns>
		Task<RedditJson<T>> GetRedditJsonAsync<T>(string uri);
	}
}