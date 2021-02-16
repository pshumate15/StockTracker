using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace StockTracker.Reddit.Authorization
{
	/// <summary>
	/// Provides functionality to authenticate to Reddit's OAuth endpoints.
	/// </summary>
	public interface IRedditAuthService
	{
		Task<AuthenticationHeaderValue> GetOAuthHeaderAsync();
	}
}