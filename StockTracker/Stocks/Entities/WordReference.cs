using StockTracker.Reddit.Entities;
using System.Collections.Generic;

namespace StockTracker.Stocks.Entities
{
	public class WordReference
	{
		public string Word { get; }

		public int ReferenceCount { get; private set; }

		public HashSet<Comment> Comments { get; }

		public WordReference(string word, int referenceCount, Comment comment)
		{
			Word = word;
			Comments = new HashSet<Comment> { comment };
			ReferenceCount = referenceCount;
		}

		public void Add(int referenceCount, Comment comment)
		{
			ReferenceCount += referenceCount;
			Comments.Add(comment);
		}
	}
}