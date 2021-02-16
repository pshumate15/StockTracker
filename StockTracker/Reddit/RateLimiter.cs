using System;
using System.Threading;
using System.Threading.Tasks;

namespace StockTracker.Reddit
{
	/// <summary>
	/// Used to limit the number of accesses to a resource to a given rate.
	/// This implementation uses a ring buffer to keep track of when elements
	/// are added to the internal buffer. This class is threadsafe.
	/// </summary>
	public class RateLimiter : IDisposable
	{
		public int RequestsStarted = 0;
		public int RequestsCompleted = 0;
		private DateTimeOffset[] _buffer;
		private TimeSpan _rateTimeSpan;
		private int _oldestIndex;
		private int _newestIndex;
		private SemaphoreSlim _bufferLock;

		public RateLimiter() : this(RedditConstants.MaxRequestsPerMinute, 60) { }

		/// <summary>
		/// Instantiates an instance of RateLimiter.
		/// </summary>
		/// <param name="capacity">The number of resource accesses to allow in a given period of time (the numerator of the rate).</param>
		/// <param name="timeSpanSeconds">The timespan in which to allow the number of resources accesses (the denominator of the rate).</param>
		public RateLimiter(int capacity, int timeSpanSeconds)
		{
			_buffer = new DateTimeOffset[capacity];
			_rateTimeSpan = TimeSpan.FromSeconds(timeSpanSeconds);
			_oldestIndex = 0;
			_newestIndex = 0;
			_bufferLock = new SemaphoreSlim(1, 1); // Allow only one access to the TryAddEventAsync method at a time
		}

		/// <summary>
		/// Asynchronously tries to add an event to the internal buffer.
		/// </summary>
		/// <returns>True if the event was successfully added, signifying that the event can continue processing.
		/// False if the event would increase the number of resource accesses above the limit.</returns>
		public async Task<bool> TryAddEventAsync()
		{
			await _bufferLock.WaitAsync();

			try
			{
				// Buffer is full if newestIndex is one spot behind oldestIndex
				if (_oldestIndex == (_newestIndex + 1) % _buffer.Length)
				{
					// The oldest element is within the timespan of the element being added
					if (DateTimeOffset.UtcNow - _buffer[_oldestIndex] < _rateTimeSpan)
					{
						return false;
					}
					else
					{
						AddNewEvent();
						_oldestIndex = (_oldestIndex + 1) % _buffer.Length;
						return true;
					}
				}
				else
				{
					AddNewEvent();
					return true;
				}
			}
			finally
			{
				_bufferLock.Release();
			}
		}

		private void AddNewEvent()
		{
			_newestIndex = (_newestIndex + 1) % _buffer.Length;
			_buffer[_newestIndex] = DateTimeOffset.UtcNow;
		}

		public void Dispose()
		{
			_bufferLock.Dispose();
		}
	}
}
