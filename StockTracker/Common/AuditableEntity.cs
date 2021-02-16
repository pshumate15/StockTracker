using System;

namespace StockTracker.Common
{
	public abstract class AuditableEntity
	{
		public DateTimeOffset SavedOn { get; set; }
	}
}