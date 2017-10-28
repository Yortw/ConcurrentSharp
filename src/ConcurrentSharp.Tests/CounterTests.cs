using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConcurrentSharp.Tests
{
	[TestClass]
	public class CounterTests
	{

		#region Constructor Tests

		[TestMethod]
		public void Counter_Contructor_DefaultsToZero()
		{
			var c = new Counter();
			Assert.AreEqual(0, (long)c);
		}

		[TestMethod]
		public void Counter_Contructor_SetsCounterToConstructorArgument()
		{
			var c = new Counter(2);
			Assert.AreEqual(2, (long)c);
		}

		#endregion

		#region Increment Tests

		[TestMethod]
		public void Counter_Increment_IncrementsValue()
		{
			var c = new Counter();
			c.Increment();
			Assert.AreEqual(1, (long)c);
			c.Increment();
			Assert.AreEqual(2, (long)c);
		}

		[TestMethod]
		public void Counter_Increment_IncrementsByValue()
		{
			var c = new Counter();
			c.Increment(5);
			Assert.AreEqual(5, (long)c);
			c.Increment(2);
			Assert.AreEqual(7, (long)c);
		}

		[TestMethod]
		public void Counter_Increment_IncrementByZeroDoesNothing()
		{
			var c = new Counter(2);
			c.Increment(0);
			Assert.AreEqual(2, (long)c);
		}

		[TestMethod]
		public void Counter_Increment_IncrementNegativeDeltaActsLikeDecrement()
		{
			var c = new Counter(2);
			c.Increment(-1);
			Assert.AreEqual(1, (long)c);
		}

		#endregion

		#region Decrement Tests

		[TestMethod]
		public void Counter_Decrement_DecrementsValue()
		{
			var c = new Counter(2);
			c.Decrement();
			Assert.AreEqual(1, (long)c);
			c.Decrement();
			Assert.AreEqual(0, (long)c);
		}

		[TestMethod]
		public void Counter_Decrement_DecrementsByValue()
		{
			var c = new Counter(7);
			c.Decrement(5);
			Assert.AreEqual(2, (long)c);
			c.Decrement(2);
			Assert.AreEqual(0, (long)c);
		}

		[TestMethod]
		public void Counter_Decrement_DecrementByZeroDoesNothing()
		{
			var c = new Counter(7);
			c.Decrement(0);
			Assert.AreEqual(7, (long)c);
		}

		[TestMethod]
		public void Counter_Decrement_DecrementNegativeDeltaWorksLikeIncrement()
		{
			var c = new Counter(7);
			c.Decrement(-1);
			Assert.AreEqual(8, (long)c);
		}

		#endregion

		#region Reset Tests

		[TestMethod]
		public void Counter_Reset_ResetsToZero()
		{
			var c = new Counter(2);
			c.Reset();
			Assert.AreEqual(0, (long)c);
		}

		#endregion

		#region Operator Tests

		[TestMethod]
		public void Counter_ImplicitLongCast_ConvertsToLong()
		{
			var counter = new Counter(102);

			long x = counter;

			Assert.AreEqual(102, x);
		}

		[TestMethod]
		public void Counter_ToInt64_ReturnsExpectedValue()
		{
			var counter = new Counter(102);

			long x = counter.ToInt64();

			Assert.AreEqual(102, x);
		}

		#endregion

	}
}