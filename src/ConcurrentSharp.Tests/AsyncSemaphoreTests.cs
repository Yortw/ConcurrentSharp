using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentSharp.Tests
{
	[TestClass]
	public class AsyncSemaphoreTests
	{

		[TestMethod]
		public void AsyncSemaphore_AsyncSynchronisesCallers()
		{
			int maxConcurrentThreads = 0;
			int concurrentThreads = 0;

			var semaphore = new AsyncSemaphore(4, 4);
			for (int cnt = 0; cnt < 10; cnt++)
			{
				System.Threading.ThreadPool.QueueUserWorkItem
				(
					async (reserved) =>
					{
						using (var token = await semaphore.WaitAsync().ConfigureAwait(false))
						{
							maxConcurrentThreads = Math.Max(maxConcurrentThreads, System.Threading.Interlocked.Increment(ref concurrentThreads));

							await Task.Delay(1000).ConfigureAwait(false);
							System.Threading.Interlocked.Decrement(ref concurrentThreads);
						}
					}
				);
			}

			System.Threading.Thread.Sleep(2000);

			Assert.AreEqual(4, maxConcurrentThreads);
		}

		[TestMethod]
		public void AsyncSemaphore_SyncSynchronisesCallers()
		{
			int maxConcurrentThreads = 0;
			int concurrentThreads = 0;

			var semaphore = new AsyncSemaphore(4, 4);
			for (int cnt = 0; cnt < 10; cnt++)
			{
				System.Threading.ThreadPool.QueueUserWorkItem
				(
					(reserved) =>
					{
						using (var token = semaphore.Wait())
						{
							maxConcurrentThreads = Math.Max(maxConcurrentThreads, System.Threading.Interlocked.Increment(ref concurrentThreads));
							System.Threading.Thread.Sleep(1000);
							System.Threading.Interlocked.Decrement(ref concurrentThreads);
						}
					}
				);
			}

			System.Threading.Thread.Sleep(2000);

			Assert.AreEqual(4, maxConcurrentThreads);
		}

		[ExpectedException(typeof(TaskCanceledException))]
		[TestMethod]
		public async Task AsyncSemaphore_CancelsWait()
		{

			var semaphore = new AsyncSemaphore(1, 1);
			var lease1 = await semaphore.WaitAsync();

			var cts = new System.Threading.CancellationTokenSource();
			System.Threading.ThreadPool.QueueUserWorkItem
			(
				(reserved) =>
				{
					System.Threading.Thread.Sleep(1000);
					cts.Cancel();
				}
			);
			var lease2 = await semaphore.WaitAsync(cts.Token);
			Assert.Fail();
		}


	}
}