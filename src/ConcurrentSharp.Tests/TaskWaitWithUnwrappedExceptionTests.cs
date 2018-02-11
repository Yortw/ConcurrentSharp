using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConcurrentSharp;
using System.Threading;

namespace ConcurrentSharp.Tests
{
	[TestClass]
	public class TaskWaitWithUnwrappedExceptionTests
	{

		[ExpectedException(typeof(InvalidOperationException))]
		[TestMethod]
		public void WaitWithUnwrappedException_UnwrapsException()
		{
			Task.Run(
				() =>
				{
					System.Threading.Thread.Sleep(100);
					throw new InvalidOperationException();
				}
			).WaitWithUnwrappedException();
		}

		[TestMethod]
		public void WaitWithUnwrappedException_ReturnsResultWhenNoException()
		{
			var result = Task.Run<int>(
				() =>
				{
					System.Threading.Thread.Sleep(100);
					return 10;
				}
			).WaitWithUnwrappedException();

			Assert.AreEqual(10, result);
		}

		[ExpectedException(typeof(OperationCanceledException))]
		[TestMethod]
		public void WaitWithUnwrappedException_ThrowsOperationCanceledExceptionWhenCancelled()
		{
			var cts = new CancellationTokenSource();

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
			).WaitWithUnwrappedException(cts.Token);
		}

	}
}