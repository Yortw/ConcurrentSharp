using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConcurrentSharp;

namespace ConcurrentSharp.Tests
{
	[TestClass]
	public class TaskTimeoutTests
	{

		[TestMethod]
		[ExpectedException(typeof(System.TimeoutException))]
		public async Task Task_Timeout_ThrowsTimeoutExceptionOnTimeout()
		{
			await Task.Delay(5000).TimeoutAfter(500);
		}

		[TestMethod]
		[ExpectedException(typeof(System.TimeoutException))]
		public async Task Task_Timeout_ThrowsTimeoutWhenTimeoutZeroAndTaskIncomplete()
		{
			await Task.Delay(5000).TimeoutAfter(0);
		}


		[TestMethod]
		public async Task Task_Timeout_DoesNotThrowTimeoutWhenTimeoutZeroAndTaskComplete()
		{
			var t = Task.Delay(100);
			await t;
			await t.TimeoutAfter(0);
		}

		[TestMethod]
		[ExpectedException(typeof(System.Threading.Tasks.TaskCanceledException))]
		public async Task Task_Timeout_ThrowsTaskCancelledWhenCancelledBeforeTimeout()
		{
			using (var cts = new System.Threading.CancellationTokenSource())
			{
				System.Threading.ThreadPool.QueueUserWorkItem((reserved) => { System.Threading.Thread.Sleep(500); cts.Cancel(); });
				await Task.Delay(5000, cts.Token).TimeoutAfter(10000);
			}
		}

		[TestMethod]
		public async Task Task_Timeout_DoesNotThrowTimeoutOnComplete()
		{
			await Task.Delay(100).TimeoutAfter(5000);
		}


		[TestMethod]
		public async Task Task_Timeout_DoesNotThrowOnInfiniteTimeout()
		{
			await Task.Delay(100).TimeoutAfter(System.Threading.Timeout.Infinite);
		}

	}
}