using System;
using System.Text.Json.Serialization;

namespace StockTracker.Reddit.Authorization
{
	/// <summary>
	/// Represents a Reddit OAuth authentication response.
	/// </summary>
	public class RedditOAuth
	{
		private int _expiresIn;

		[JsonPropertyName("access_token")]
		public string AccessToken { get; set; }

		[JsonPropertyName("token_type")]
		public string TokenType { get; set; }

		[JsonPropertyName("expires_in")]
		public int ExpiresIn
		{
			get => _expiresIn;
			set
			{
				_expiresIn = value;
				Expiration = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(value);
			}
		}

		[JsonPropertyName("scope")]
		public string Scope { get; set; }

		public DateTimeOffset Expiration { get; set; }
	}
}