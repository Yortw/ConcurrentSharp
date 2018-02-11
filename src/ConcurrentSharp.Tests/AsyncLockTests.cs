using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentSharp.Tests
{
	[TestClass]
	public class AsyncLockTests
	{


		[TestMethod]
		public void AsyncLock_SynchronisesThreads()
		{
			int concurrentTasks = 0;
			bool failed = false;

			var asyncLock = new AsyncLock();
			
			for (int cnt = 0; cnt < 10; cnt++)
			{
				System.Threading.ThreadPool.QueueUserWorkItem
				(
					async (reserved) =>
					{
						using (var token = await asyncLock.LockAsync().ConfigureAwait(false))
						{
							System.Threading.Interlocked.Increment(ref concurrentTasks);
							System.Threading.Thread.Sleep(1000);
							if (concurrentTasks > 1)
								failed = true;

							System.Threading.Interlocked.Decrement(ref concurrentTasks);
						}
					}
				);
			}

			System.Threading.Thread.Sleep(2000);

			Assert.IsFalse(failed);
		}

	}
}