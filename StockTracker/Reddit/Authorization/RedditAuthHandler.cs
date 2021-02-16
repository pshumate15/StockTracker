using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace StockTracker.Reddit.Authorization
{
	public class RedditAuthHandler : DelegatingHandler
	{
		private readonly IRedditAuthService _authService;

		public RedditAuthHandler(IRedditAuthService authService)
		{
			_authService = authService;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			request.Headers.Authorization = await _authService.GetOAuthHeaderAsync();
			return await base.SendAsync(request, cancellationToken);
		}
	}
}
