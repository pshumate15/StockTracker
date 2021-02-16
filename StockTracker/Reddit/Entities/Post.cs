using StockTracker.Common;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StockTracker.Reddit.Entities
{
	public class Post : AuditableEntity
	{
		public int PostID { get; set; }

		[JsonPropertyName("id")]
		public string RedditID { get; set; }

		[JsonPropertyName("title")]
		public string Title { get; set; }

		[JsonPropertyName("author")]
		public string Author { get; set; }

		[JsonPropertyName("permalink")]
		public string Permalink { get; set; }

		[JsonPropertyName("created_utc")]
		public double RedditCreatedOnUtc { get; set; }

		public Subreddit Subreddit { get; set; }

		public List<Comment> Comments { get; set; }
	}
}