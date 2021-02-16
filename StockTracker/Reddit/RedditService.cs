using Microsoft.Extensions.Logging;
using StockTracker.Reddit.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockTracker.Reddit
{
    /// <inheritdoc cref="IRedditService"/>
    public class RedditService : IRedditService
    {
        private readonly IRedditHttpService _httpService;
        private readonly ILogger<RedditService> _logger;

        public RedditService(IRedditHttpService httpService, ILogger<RedditService> logger)
        {
            _httpService = httpService;
            _logger = logger;
        }

        /// <inheritdoc/>
		public async Task<List<Post>> GetPostsAsync(Subreddit subreddit, string sort = "hot", int limit = 100)
        {
            var sortAndLimit = $"{sort}?limit={limit}";

            if (!subreddit.Permalink.EndsWith('/'))
            {
                sortAndLimit = '/' + sortAndLimit;
            }

            RedditJson<Post> redditJson;

            try
            {
                redditJson = await _httpService.GetRedditJsonAsync<Post>(subreddit.Permalink + sortAndLimit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting posts for subreddit {Subreddit}", subreddit.Name);
                return new List<Post>();
            }

            var posts = redditJson.GetChildren(RedditConstants.JsonPostsIndex);

            foreach (var post in posts)
            {
                post.Subreddit = subreddit;
            }

            return posts;
        }

        /// <inheritdoc/>
        public async Task<List<Comment>> GetCommentsAsync(List<Post> posts)
        {
            var getCommentsTasks = new List<Task<List<Comment>>>();

            foreach (var post in posts)
            {
                var getCommentsTask = GetCommentsAsync(post);
                getCommentsTasks.Add(getCommentsTask);
            }

            await Task.WhenAll(getCommentsTasks);
            return getCommentsTasks.SelectMany(t => t.Result).ToList();
        }

        /// <inheritdoc/>
        public async Task<List<Comment>> GetCommentsAsync(Post post)
        {
            RedditJson<Comment> redditJson;

            // Get top level comments
            try
            {
                redditJson = await _httpService.GetRedditJsonAsync<Comment>(post.Permalink);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while getting comments for post {Post}", post);
                return new List<Comment>();
            }

            var comments = new List<Comment>();

            foreach (var comment in redditJson.GetChildren(RedditConstants.JsonCommentsIndex))
            {
                comment.Post = post;
                comments.Add(comment);
                comments.AddRange(comment.GetReplies());
            }

            return comments;
        }
    }
}