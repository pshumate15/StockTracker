using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using StockTracker.Infrastructure;
using StockTracker.Reddit;
using StockTracker.Reddit.Entities;
using StockTracker.Stocks.Entities;

namespace StockTracker.Stocks
{
	/// <inheritdoc cref="IStocksService"/>
	public class StocksService : IStocksService
	{
		private readonly IRedditService _reddit;
		private readonly StockReferenceContext _context;
		private readonly ILogger<StocksService> _logger;

		private Dictionary<string, Stock> stocksBySymbol;

		public StocksService(IRedditService reddit, StockReferenceContext context, ILogger<StocksService> logger)
		{
			_reddit = reddit;
			_context = context;
			_logger = logger;
		}

		/// <inheritdoc/>
		public async Task ProcessStockReferencesAsync()
		{
			var subreddits = await _context.Subreddits.ToListAsync();
			var stockRefTasks = new List<Task<List<StockReference>>>();

			foreach (var subreddit in subreddits)
			{
				stockRefTasks.Add(GetStockReferencesAsync(subreddit));
			}

			await Task.WhenAll(stockRefTasks);
			var stockRefs = stockRefTasks.SelectMany(t => t.Result).ToList();

			// May be more performant (and more straightforward?) to save via a SQL proc, 
			// which would reduce the number of trips to the DB since we wouldn't
			// need to return whether a RedditID already exists
			await RemoveExistingStockRefsAsync(stockRefs);
			await PopulatePostDataAsync(stockRefs);

			_logger.LogInformation("New Stock Reference Count: {NewStockReferencesCount}", stockRefs.Count);

			_context.StockReferences.AddRange(stockRefs);
			await _context.SaveChangesAsync();
		}

		/// <inheritdoc/>
		public async Task<List<StockReference>> GetStockReferencesAsync(Subreddit subreddit)
		{
			using (LogContext.PushProperty("Subreddit", '[' + subreddit.Name + "] "))
			{
				var getWordRefsTask = GetWordRefsFromPostsAsync(subreddit);
				Dictionary<string, Stock> stocksBySymbol = await GetStocksBySymbol();
				var wordRefsByWord = await getWordRefsTask;
				var stockRefs = new List<StockReference>();

				foreach (var stock in stocksBySymbol)
				{
					if (wordRefsByWord.TryGetValue(stock.Key, out WordReference refGroup))
					{
						foreach (var comment in refGroup.Comments)
						{
							stockRefs.Add(new StockReference
							{
								Stock = stock.Value,
								Comment = comment
							});
						}
					}
				}

				_logger.LogInformation("Stock Reference Count: {StockReferenceCount}", stockRefs.Count);

				return stockRefs;
			}
		}

		private async Task<Dictionary<string, WordReference>> GetWordRefsFromPostsAsync(Subreddit subreddit, string sort = "hot")
		{
			var posts = await _reddit.GetPostsAsync(subreddit, sort);
			var comments = await _reddit.GetCommentsAsync(posts);

			_logger.LogInformation("Post Count: {PostCount}; Comment Count: {CommentCount}", posts.Count, comments.Count);

			var wordRefsByWord = new Dictionary<string, WordReference>();
			// That regex...
			var regex = new Regex(@"(?<=[\W]{1}\${1})([a-zA-Z]{1,4})(?=[\.\!\?\s\\])|(?<=\s{1})([A-Z]{2,4})(?=[\.\!\?\s\\])", 
				RegexOptions.Compiled);

			foreach (var comment in comments)
			{
				if (comment.Body == null)
				{
					continue;
				}

				var matches = regex.Matches(comment.Body);

				foreach (Match match in matches)
				{
					if (wordRefsByWord.TryGetValue(match.Value, out WordReference wordRef))
					{
						wordRef.Add(1, comment);
					}
					else
					{
						wordRefsByWord.Add(match.Value, new WordReference(match.Value, 1, comment));
					}
				}
			}

			return wordRefsByWord;
		}

		private async Task RemoveExistingStockRefsAsync(List<StockReference> stockRefs)
		{
			// Remove the stock references where the comment already exists in the db
			var commentRedditIDs = stockRefs.Select(s => s.Comment.RedditID);
			var existingRedditIDs = await _context.Comments
				.Where(c => commentRedditIDs.Contains(c.RedditID))
				.Select(c => c.RedditID)
				.ToListAsync();
			stockRefs.RemoveAll(s => existingRedditIDs.Contains(s.Comment.RedditID));
		}

		private async Task PopulatePostDataAsync(List<StockReference> stockRefs)
		{
			var postRedditIDs = stockRefs.Select(s => s.Comment.Post.RedditID);
			var existingPosts = await _context.Posts
				.Where(p => postRedditIDs.Contains(p.RedditID))
				.ToDictionaryAsync(p => p.RedditID);

			foreach (var stockRef in stockRefs)
			{
				if (existingPosts.TryGetValue(stockRef.Comment.Post.RedditID, out Post post))
				{
					stockRef.Comment.Post = post;
				}
			}
		}

		private async Task<Dictionary<string, Stock>> GetStocksBySymbol()
		{
			if (stocksBySymbol == null)
			{
				stocksBySymbol = await _context.Stocks.ToDictionaryAsync(s => s.Symbol);
			}

			return stocksBySymbol;
		}
	}
}
