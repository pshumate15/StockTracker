using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StockTracker.Reddit.Entities
{
	/// <summary>
	/// A Reddit listing of type T.
	/// </summary>
	/// <typeparam name="T">The type of objects in this listing (e.g. comment or post).</typeparam>
	public class Listing<T>
	{
		[JsonPropertyName("before")]
		public string Before { get; set; }

		[JsonPropertyName("after")]
		public string After { get; set; }

		[JsonPropertyName("children")]
		public List<Thing<T>> Children { get; set; }
	}
}
