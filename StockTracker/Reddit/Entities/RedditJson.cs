using System.Collections.Generic;
using System.Linq;

namespace StockTracker.Reddit.Entities
{
	// See https://github.com/reddit-archive/reddit/wiki/JSON for Reddit's response layout
	public class RedditJson<T>
	{
		public Thing<Listing<T>>[] Listing { get; set; }

		public List<T> GetChildren(int listingIndex)
		{
			return Listing[listingIndex].Data.Children.Select(c => c.Data).ToList();
		}
	}
}
