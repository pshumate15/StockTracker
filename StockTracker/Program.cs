using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using StockTracker.Common;
using StockTracker.Infrastructure;
using StockTracker.Reddit;
using StockTracker.Reddit.Authorization;
using StockTracker.Stocks;
using System;
using System.Threading.Tasks;

namespace StockTracker
{
	public class Program
	{
		public static IConfigurationRoot Configuration { get; set; }

		public static async Task<int> Main(string[] args)
		{
			string outputTemplate = "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] " +
				"[{SourceContext}] {Subreddit}{Message:lj}{NewLine}{Exception}";

			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
				.MinimumLevel.Override("System", LogEventLevel.Warning)
				.Enrich.FromLogContext()
				.Enrich.WithExceptionDetails()
				.WriteTo.Console(outputTemplate: outputTemplate)
				.WriteTo.File($"{AppContext.BaseDirectory}/Logs/log.txt", outputTemplate: outputTemplate, rollingInterval: RollingInterval.Day)
				.CreateLogger();

			try
			{
				var devEnvironmentVariable = Environment.GetEnvironmentVariable("NETCORE_ENVIRONMENT");
				var isDevelopment = string.IsNullOrEmpty(devEnvironmentVariable)
					|| devEnvironmentVariable.ToLower() == "development";
				var builder = new ConfigurationBuilder();

				if (isDevelopment)
				{
					builder.AddUserSecrets<Program>();
				}

				Configuration = builder.Build();

				Log.Information("Starting host");

				var host = Host.CreateDefaultBuilder(args)
					.ConfigureServices((context, services) =>
					{
						services.Configure<Secrets>(Configuration.GetSection(nameof(Secrets)));

						services.AddDbContext<StockReferenceContext>();

						services.AddSingleton<RateLimiter>();
						services.AddSingleton<IRedditAuthService, RedditAuthService>();
						services.AddTransient<IRedditService, RedditService>();
						services.AddTransient<RedditAuthHandler>();
						services.AddTransient<IStocksService, StocksService>();

						services.AddHttpClient<IRedditHttpService, RedditHttpService>()
							.AddHttpMessageHandler<RedditAuthHandler>();
					})
					.UseSerilog()
					.Build();

				using (var scope = host.Services.CreateScope())
				{
					var service = scope.ServiceProvider.GetService<IStocksService>();
					await service.ProcessStockReferencesAsync();
				}

				return 0;
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Host terminated unexpectedly");
				return 1;
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}
	}
}
