using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentSharp.Tests
{
	[TestClass]
	public class TaskWaitExceptionless
	{

		[TestMethod]
		public void WaitExceptionless_IgnoresExceptions()
		{
			Task.Run(
				() =>
				{
					System.Threading.Thread.Sleep(100);
					throw new InvalidOperationException();
				}
			).WaitExceptionless();
		}

		[TestMethod]
		public void WaitExceptionless_ReturnsResultWhenNoException()
		{
			var result = Task.Run<int>(
				() =>
				{
					System.Threading.Thread.Sleep(100);
					return 10;
				}
			).WaitExceptionless();

			Assert.AreEqual(10, result);
		}

		[TestMethod]
		public void WaitExceptionless_ReturnsDefaultValueWhenExceptionThrown()
		{
			var result = Task.Run<int>
			(
				new Func<int>(
					() =>
					{
						System.Threading.Thread.Sleep(100);
						throw new InvalidOperationException();
					}
				)
			).WaitExceptionless();

			Assert.AreEqual(0, result);
		}


		[ExpectedException(typeof(OperationCanceledException))]
		[TestMethod]
		public void WaitExceptionless_ThrowsTaskCanceledExceptionWhenWaitCancelled()
		{
			var cts = new System.Threading.CancellationTokenSource();

			System.Threading.ThreadPool.QueueUserWorkItem
			(
				(reserved) =>
				{
					System.Threading.Thread.Sleep(100);
					cts.Cancel();
				}
			);

			Task.Run(
				() =>
				{
					System.Threading.Thread.Sleep(10000);
					throw new InvalidOperationException();
				}
			).WaitExceptionless(cts.Token);
		}

	}
}