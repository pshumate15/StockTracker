using System.Text.Json.Serialization;

namespace StockTracker.Reddit.Entities
{
	// Corresponds with Reddit's "Thing" JSON object
	public class Thing<T>
	{
		[JsonPropertyName("id")]
		public string Id { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("kind")]
		public string Kind { get; set; }

		[JsonPropertyName("data")]
		public T Data { get; set; }
	}
}
