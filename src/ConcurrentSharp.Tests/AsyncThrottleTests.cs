using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentSharp.Tests
{
	[TestClass]
	public class AsyncThrottleTests
	{
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
			using (var t = new AsyncThrottle(4))
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

			using (var t = new AsyncThrottle(4))
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