using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using StockTracker.Common;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace StockTracker.Reddit.Authorization
{
    /// <inheritdoc cref="IRedditAuthService"/>
    public class RedditAuthService : IRedditAuthService, IDisposable
    {
		private readonly string _redditSecret;
		private readonly IHttpClientFactory _httpClientFactory;
        private RedditOAuth _redditOAuth;
        private SemaphoreSlim _authLock;

        public RedditAuthService(IOptions<Secrets> secrets, IHttpClientFactory httpClientFactory)
        {
            _redditSecret = secrets.Value.RedditSecret;
            _httpClientFactory = httpClientFactory;
            // Allow only one request to get auth data at a time 
            // to prevent multiple requests from trying to authenticate simultaneously
            _authLock = new SemaphoreSlim(1, 1);
        }

        /// <summary>
        /// Gets the Reddit OAuth header for requests to oauth.reddit.com.
        /// If the Bearer token hasn't been generated yet or is expired, the method will re-authenticate.
        /// </summary>
        /// <returns>An OAuth header that can be used for requests to oauth.reddit.com.</returns>
        public async Task<AuthenticationHeaderValue> GetOAuthHeaderAsync()
        {
            await _authLock.WaitAsync();

            try
            {
                if (_redditOAuth != null && DateTimeOffset.UtcNow < _redditOAuth.Expiration)
                {
                    return new AuthenticationHeaderValue("Bearer", _redditOAuth.AccessToken);
                }

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("User-Agent", RedditConstants.UserAgent);
                httpClient.DefaultRequestHeaders.Authorization =
                    GetBasicAuthRequestHeader(RedditConstants.ClientId, _redditSecret);
                var basicAuthRequest = GetBasicAuthRequestContent();
                var authResponse = await httpClient.PostAsync("https://www.reddit.com/api/v1/access_token", basicAuthRequest);
                var contentStream = await authResponse.Content.ReadAsStreamAsync();
                _redditOAuth = await JsonSerializer.DeserializeAsync<RedditOAuth>(contentStream);
            }
            finally
            {
                _authLock.Release();
            }

            return new AuthenticationHeaderValue("Bearer", _redditOAuth.AccessToken);
        }

        private AuthenticationHeaderValue GetBasicAuthRequestHeader(string clientId, string secret)
        {
            var creds = clientId + ":" + secret;
            var credsByteArray = Encoding.ASCII.GetBytes(creds);
            var credsBase64 = Convert.ToBase64String(credsByteArray);
            return new AuthenticationHeaderValue("Basic", credsBase64);
        }

        private FormUrlEncodedContent GetBasicAuthRequestContent()
        {
            var content = new Dictionary<string, string>();
            content.Add("grant_type", "client_credentials");
            return new FormUrlEncodedContent(content);
        }

        public void Dispose()
        {
            _authLock.Dispose();
        }
    }
}
