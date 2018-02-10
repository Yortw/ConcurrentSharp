using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentSharp.Tests
{
	[TestClass]
	public class ThrottleTests
	{

		#region Constructor Tests

		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		[TestMethod]
		public void Throttle_Constructor_ThrowsIfMaxConcurrencyLessThanZero()
		{
			var throttle = new Throttle(-1);
		}

		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		[TestMethod]
		public void Throttle_Constructor_ThrowsIfMaxConcurrencyIsZero()
		{
			var throttle = new Throttle(0);
		}

		[TestMethod]
		public void Throttle_Constructor_ConstructsWithPositiveMaxConcurrencyValue()
		{
			using (var throttle = new Throttle(4))
			{
			}
		}

		#endregion

		#region Enter Tests

		[TestMethod]
		public void Throttle_Enter_RestrictsConcurrentOperations()
		{
			int concurrentOps = 0;
			using (var throttle = new Throttle(1))
			{
				using (var signalOne = new System.Threading.ManualResetEvent(false))
				using (var signalTwo = new System.Threading.ManualResetEvent(false))
				{
					System.Threading.ThreadPool.QueueUserWorkItem((reserved) =>
					{
						using (var token = throttle.Enter())
						{
							System.Threading.Interlocked.Increment(ref concurrentOps);
							signalOne.WaitOne();
							System.Threading.Interlocked.Decrement(ref concurrentOps);
						}
					});

					System.Threading.ThreadPool.QueueUserWorkItem((reserved) =>
					{
						using (var token = throttle.Enter())
						{
							System.Threading.Interlocked.Increment(ref concurrentOps);
							signalTwo.WaitOne();
							System.Threading.Interlocked.Decrement(ref concurrentOps);
						}
					});

					System.Threading.Thread.Sleep(100);
					System.Threading.Thread.Yield();
					Assert.AreEqual(1, concurrentOps);

					signalOne.Set();
					System.Threading.Thread.Sleep(100);
					System.Threading.Thread.Yield();
					Assert.AreEqual(1, concurrentOps);
					signalTwo.Set();

					System.Threading.Thread.Sleep(100);
					System.Threading.Thread.Yield();
					Assert.AreEqual(0, concurrentOps);
				}
			}
		}

		[ExpectedException(typeof(ObjectDisposedException))]
		[TestMethod]
		public void Throttle_Enter_ThrowsWhenDisposed()
		{
			var throttle = new Throttle(2);
			throttle.Dispose();

			using (var token = throttle.Enter())
			{
			}
		}

		#endregion

		[TestMethod]
		public void Throttle_Execute_ExecutesAction()
		{
			bool wasCalled = false;

			using (var throttle = new Throttle(1))
			{
				throttle.Execute(() => wasCalled = true);
			}

			Assert.AreEqual(true, wasCalled);
		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public void Throttle_Execute_ThrowsOnNullAction()
		{
			using (var throttle = new Throttle(1))
			{
				throttle.Execute((Action)null);
			}

			Assert.Fail();
		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public void Throttle_Execute_ThrowsOnNullFunction()
		{
			using (var throttle = new Throttle(1))
			{
				var result = throttle.Execute((Func<int>)null);
				Assert.Fail();
			}
		}

		[TestMethod]
		public void Throttle_Execute_ReturnsFunctionResult()
		{
			using (var throttle = new Throttle(1))
			{
				var result = throttle.Execute(() => 6);
				Assert.AreEqual(6, result);
			}
		}

		[TestMethod]
		public async Task Throttle_ExecuteAllAsyncAction_Test()
		{
			var items = new List<int>(10);
			items.Add(1);
			items.Add(2);
			items.Add(3);
			items.Add(4);
			items.Add(5);
			items.Add(6);
			items.Add(7);
			items.Add(8);
			items.Add(9);
			items.Add(10);

			int tasksExecuted = 0;
			using (var t = new Throttle(4))
			{
				await t.ExecuteAllAsync(items, async (i) =>
				{
					await Task.Delay(100).ConfigureAwait(false);
					System.Threading.Interlocked.Increment(ref tasksExecuted);
				});

				Assert.AreEqual(10, tasksExecuted);
			}
		}

		[TestMethod]
		public async Task Throttle_ExecuteAllAsyncFunc_Test()
		{
			var items = new List<int>(10);
			items.Add(1);
			items.Add(2);
			items.Add(3);
			items.Add(4);
			items.Add(5);
			items.Add(6);
			items.Add(7);
			items.Add(8);
			items.Add(9);
			items.Add(10);

			using (var t = new Throttle(4))
			{
				var results = await t.ExecuteAllAsync<int, int>(items, async (i) =>
				{
					await Task.Delay(100).ConfigureAwait(false);
					return ++i;
				});

				Assert.AreEqual(10, results.Count());

				int oi = 1;
				foreach (var i in results)
				{
					Assert.AreEqual(oi + 1, i.Result);
					oi++;
				}
			}
		}
	}
}