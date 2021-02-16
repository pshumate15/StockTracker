
using StockTracker.Reddit.Entities;
using StockTracker.Stocks.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockTracker.Stocks
{
	/// <summary>
	/// Provides functionality for getting stock reference data.
	/// </summary>
	public interface IStocksService
	{
		/// <summary>
		/// Gets stock reference data for a given subreddit.
		/// </summary>
		/// <returns>A list of stock references that contain data on posts and comments that mention each stock.</returns>
		Task<List<StockReference>> GetStockReferencesAsync(Subreddit subreddit);

		/// <summary>
		/// Gets and saves stock reference data for all of the subreddits in the DB.
		/// </summary>
		Task ProcessStockReferencesAsync();
	}
}