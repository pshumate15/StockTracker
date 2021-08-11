using Microsoft.EntityFrameworkCore;
using StockTracker.Common;
using StockTracker.Reddit.Entities;
using StockTracker.Stocks.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StockTracker.Infrastructure
{
	public class StockReferenceContext : DbContext
	{
		public DbSet<Stock> Stocks { get; set; }
		public DbSet<StockReference> StockReferences { get; set; }
		public DbSet<Subreddit> Subreddits { get; set; }
		public DbSet<Post> Posts { get; set; }
		public DbSet<Comment> Comments { get; set; }

		public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
		{
			foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
			{
				if (entry.State == EntityState.Added)
				{
					entry.Entity.SavedOn = DateTimeOffset.UtcNow;
				}
			}

			return base.SaveChangesAsync(cancellationToken);
		}

		protected override void OnConfiguring(DbContextOptionsBuilder options)
			=> options.UseSqlite($"Data Source={AppContext.BaseDirectory}/StockTracker.db");

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<Comment>().Ignore(c => c.Replies);

			base.OnModelCreating(modelBuilder);
		}
	}
}
