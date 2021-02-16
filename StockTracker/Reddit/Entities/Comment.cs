using StockTracker.Common;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StockTracker.Reddit.Entities
{
	/// <summary>
	/// Represents a Reddit comment.
	/// </summary>
	public class Comment : AuditableEntity
	{
		public int CommentID { get; set; }

		[JsonPropertyName("id")]
		public string RedditID { get; set; }

		[JsonPropertyName("author")]
		public string Author { get; set; }

		[JsonPropertyName("body")]
		public string Body { get; set; }

		[JsonPropertyName("created_utc")]
		public double RedditCreatedOnUtc { get; set; }

		[JsonPropertyName("replies")]
		public Thing<Listing<Comment>> Replies { get; set; }

		public Post Post { get; set; }

		/// <summary>
		/// Recursively retrieves all comments in this comment's subtree.
		/// </summary>
		/// <param name="addSelf">If true, the calling object will add itself to the list of comments being returned.</param>
		/// <param name="post">The post that contains this comment.</param>
		/// <returns>A list of comments in this comment's subtree.</returns>
		public List<Comment> GetReplies(bool addSelf = false, Post post = null)
		{
			var comments = new List<Comment>();

			if (addSelf)
			{
				Post = post;
				comments.Add(this);
			}

			if (Replies?.Data?.Children != null)
			{
				foreach (var thing in Replies.Data.Children)
				{
					if (thing.Kind == "t1") // t1 is the comment type in the json response
					{
						comments.AddRange(thing.Data.GetReplies(true, Post));
					}
				}
			}

			return comments;
		}
	}
}
