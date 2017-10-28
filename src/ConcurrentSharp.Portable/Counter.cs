using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConcurrentSharp
{
	/// <summary>
	/// Provides thread-safe operations around a signed long integer.
	/// </summary>
	/// <remarks>
	/// <para>While <see cref="Counter"/> instances can be converted to/read as a long integer value, correct usage for multi-threading requires using the return value of the instance methods to obtain the accurate counter value.
	/// Treating the counter as a long integer value is only useful where an inaccurate, at a moment in time value, is ok such us updating a display with the 'current' value.</para>
	/// </remarks>
	public class Counter
	{

		#region Fields

		private long _Value;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs a new counter with a value of zero.
		/// </summary>
		public Counter()
		{
		}

		/// <summary>
		/// Creates a new counter with an initial values of <paramref name="value"/>.
		/// </summary>
		/// <param name="value">A long integer specifying the initial integer value.</param>
		public Counter(long value)
		{
			_Value = value;
		}

		#endregion

		#region Public Methods

		#region Increment Overloads

		/// <summary>
		/// Increments the counter by one and returns the value of the counter immediately after the increment, as an atomic, thread-safe operation.
		/// </summary>
		/// <remarks>
		/// <para>If the value of the counter overflows the <see cref="Int64.MaxValue"/> then the counter value will wrap around to <see cref="Int64.MinValue"/>.</para>
		/// </remarks>
		/// <returns>The value of the counter immediately after the increment occurred.</returns>
		/// <example>
		/// <code>
		/// var newValue = counter.Increment();
		/// </code>
		/// </example>
		public long Increment()
		{
			return System.Threading.Interlocked.Increment(ref _Value);
		}

		/// <summary>
		/// Increments the counter by <paramref name="delta"/> and returns the value of the counter immediately after the increment, as an atomic, thread-safe operation.
		/// </summary>
		/// <remarks>
		/// <para>If <paramref name="delta"/> is zero, no change occurs. If <paramref name="delta"/> is negative, this is equivalent to calling <see cref="Decrement(long)"/>.</para>
		/// <para>If the value of the counter overflows the <see cref="Int64.MaxValue"/> then the counter value will wrap around to <see cref="Int64.MinValue"/>.</para>
		/// </remarks>
		/// <param name="delta">The amount to increment the counter by.</param>
		/// <returns></returns>
		/// <example>
		/// <code>
		/// var newValue = counter.Increment(5);
		/// </code>
		/// </example>
		public long Increment(long delta)
		{
			if (delta == 0) return System.Threading.Interlocked.Read(ref _Value);

			return System.Threading.Interlocked.Add(ref _Value, delta);
		}

		#endregion

		#region Decrement Overloads

		/// <summary>
		/// Decrements the counter by one and returns the value of the counter immediately after the increment, as an atomic, thread-safe operation.
		/// </summary>
		/// <remarks>
		/// <para>If the value of the counter underflows the <see cref="Int64.MinValue"/> then the counter value will wrap around to <see cref="Int64.MaxValue"/>.</para>
		/// </remarks>
		/// <returns>The value of the counter immediately after the decrement occurred.</returns>
		/// <example>
		/// <code>
		/// var newValue = counter.Decrement();
		/// </code>
		/// </example>
		public long Decrement()
		{
			return System.Threading.Interlocked.Decrement(ref _Value);
		}

		/// <summary>
		/// Decrements the counter by <paramref name="delta"/> and returns the value of the counter immediately after the decrement, as an atomic, thread-safe operation.
		/// </summary>
		/// <remarks>
		/// <para>If <paramref name="delta"/> is zero, no change occurs. If <paramref name="delta"/> is negative, this is equivalent to calling <see cref="Increment(long)"/>.</para>
		/// <para>If the value of the counter underflows the <see cref="Int64.MinValue"/> then the counter value will wrap around to <see cref="Int64.MaxValue"/>.</para>
		/// </remarks>
		/// <param name="delta">The amount to decrement the counter by.</param>
		/// <returns></returns>
		/// <example>
		/// <code>
		/// var newValue = counter.Decrement(5);
		/// </code>
		/// </example>
		public long Decrement(long delta)
		{
			if (delta == 0) return System.Threading.Interlocked.Read(ref _Value);

			return System.Threading.Interlocked.Add(ref _Value, delta * -1);
		}

		#endregion

		/// <summary>
		/// Resets the counter to a value of zero, as an atomic, thread-safe operation.
		/// </summary>
		/// <example>
		/// <code>
		/// counter.Reset(5);
		/// </code>
		/// </example>
		public void Reset()
		{
			System.Threading.Interlocked.Exchange(ref _Value, 0);
		}

		#endregion

		#region Operators

		/// <summary>
		/// Returns the 'current' value of the counter as a long integer, using an atomic/thread-safe read.
		/// </summary>
		/// <param name="counter">The <see cref="Counter"/> instance to read the value of.</param>
		public static implicit operator long(Counter counter)
		{
			if (counter == null) return 0;

			return System.Threading.Interlocked.Read(ref counter._Value);
		}

		/// <summary>
		/// Returns the value of the counter as an <see cref="Int64"/>, provided for languages that don't support the implicit cast operator overload.
		/// </summary>
		/// <returns>An <see cref="Int64"/> (long) integer value containing the 'current' value of the counter.</returns>
		public long ToInt64()
		{
			return System.Threading.Interlocked.Read(ref _Value);
		}

		#endregion

	}
}