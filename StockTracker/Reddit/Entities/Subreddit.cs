using StockTracker.Common;
using System.Collections.Generic;

namespace StockTracker.Reddit.Entities
{
	public class Subreddit : AuditableEntity
	{
		public int SubredditID { get; set; }

		public string Name { get; set; }

		public string Permalink { get; set; }

		public List<Post> Posts { get; set; }
	}
}
