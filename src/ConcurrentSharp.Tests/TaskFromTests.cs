using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentSharp.Tests
{
	[TestClass]
	public class TaskFromTests
	{

		#region From Event Tests

		[TestMethod]
		[Timeout(6000)]
		public async Task TaskFrom_EventHandler_TaskCompletesOnEvent()
		{
			var eventSource = new TestEventSource();

			Task.Delay(1000).ContinueWith((pt) => eventSource.OnTestEvent1()).Ignore();

			await TaskFrom.Event(eventSource, "TestEvent1");
		}

		[TestMethod]
		[Timeout(6000)]
		public async Task TaskFrom_EventHandlerOfT_TaskCompletesOnEvent()
		{
			var eventSource = new TestEventSource();

			Task.Delay(1000).ContinueWith((pt) => eventSource.OnTestEvent2()).Ignore();

			var args = await TaskFrom.Event<TestEventSource, CancelEventArgs>(eventSource, "TestEvent2");
			Assert.AreEqual(true, args.Cancel);
		}

		#endregion

		#region From AsyncCallback Tests

		[TestMethod]
		public async Task From_AsyncCallback_TaskCompletesOnCallback()
		{
			var ms = new System.IO.MemoryStream(100);

			var buffer = System.Text.UTF8Encoding.UTF8.GetBytes("Hello!");
			await TaskFrom.FromAsyncCallback<byte[], int, int>(ms.BeginWrite, ms.EndWrite, buffer, 0, buffer.Length);

			Assert.AreEqual(ms.Length, 6);
			ms.Seek(0, System.IO.SeekOrigin.Begin);
			using (var reader = new System.IO.StreamReader(ms))
			{
				Assert.AreEqual(await reader.ReadToEndAsync(), "Hello!");
			}
		}

		[TestMethod]
		public async Task From_AsyncCallback_TaskCompletesAndReturnsValueOnCallback()
		{
			var ms = new System.IO.MemoryStream(System.Text.UTF8Encoding.UTF8.GetBytes("Hello!"));

			var buffer = new byte[6];
			var bytesRead = await TaskFrom.FromAsyncCallback<byte[], int, int, int>(ms.BeginRead, ms.EndRead, buffer, 0, buffer.Length);

			Assert.AreEqual(6, bytesRead);
			ms.Seek(0, System.IO.SeekOrigin.Begin);
			using (var reader = new System.IO.StreamReader(ms))
			{
				Assert.AreEqual(System.Text.UTF8Encoding.UTF8.GetString(buffer), "Hello!");
			}
		}

		#endregion

	}

	public class TestEventSource
	{

		public event EventHandler TestEvent1;
		public event EventHandler<CancelEventArgs> TestEvent2;

		public void OnTestEvent1()
		{
			TestEvent1?.Invoke(this, EventArgs.Empty);
		}

		public void OnTestEvent2()
		{
			TestEvent2?.Invoke(this, new CancelEventArgs(true));
		}

	}
}