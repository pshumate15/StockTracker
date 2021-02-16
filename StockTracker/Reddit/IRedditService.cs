using StockTracker.Reddit.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StockTracker.Reddit
{
	/// <summary>
	/// Provides functionality for interacting with and storing information from Reddit.
	/// </summary>
	public interface IRedditService
	{
		/// <summary>
		/// Gets the posts from the given subreddit.
		/// </summary>
		/// <param name="subreddit"></param>
		/// <param name="sort">The sort of the subreddit posts.</param>
		/// <param name="limit">The max number of posts to return. The max is 100.</param>
		/// <returns>A list of posts at the given URI.</returns>
		Task<List<Post>> GetPostsAsync(Subreddit subreddit, string sort = "hot", int limit = 100);

		/// <summary>
		/// Gets the comments from the given list of posts.
		/// </summary>
		/// <param name="posts">The list of posts whose comments should be retrieved.</param>
		/// <returns>A list of comments found in the list of posts.</returns>
		Task<List<Comment>> GetCommentsAsync(List<Post> posts);

		/// <summary>
		/// Gets the comments from a given post.
		/// </summary>
		/// <param name="post">The post whose comments should be retrieved.</param>
		/// <returns>A list of comments found in the post.</returns>
		Task<List<Comment>> GetCommentsAsync(Post post);
	}
}