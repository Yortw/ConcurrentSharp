using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentSharp.Tests
{
	[TestClass]
	public class BlockingCollectionProcessorTests
	{

		[TestMethod]
		public void BlockingCollectionProcessor_Instance_ProcessesAllItems()
		{
			int testItemCount = 100;

			var allItems = new List<Item>(testItemCount);
			var collection = new System.Collections.Concurrent.BlockingCollection<Item>();
			using (var processor = new BlockingCollectionProcessor<Item>(collection, 4,
				(item) =>
				{
					System.Threading.Thread.Sleep(100);
					item.Processed = true;
				},
				ExpectedThreadLifetime.Short
			))
			{
				for (int cnt = 0; cnt < testItemCount; cnt++)
				{
					var i = new Item();
					allItems.Add(i);
					collection.Add(i);
				}

				collection.CompleteAdding();
				processor.WaitForCompletion();
				Assert.AreEqual(0, collection.Count);
				Assert.AreEqual(testItemCount, allItems.Count);
				Assert.IsFalse((from i in allItems where !i.Processed select i).Any());
				Assert.IsTrue(collection.IsCompleted);
			}

		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public void BlockingCollectionProcessor_Instance_ThrowsOnNullCollection()
		{
			int testItemCount = 100;

			var allItems = new List<Item>(testItemCount);
			using (var processor = new BlockingCollectionProcessor<Item>(null, 4,
				(item) =>
				{
					System.Threading.Thread.Sleep(100);
					item.Processed = true;
				},
				ExpectedThreadLifetime.Short
			))
			{
				for (int cnt = 0; cnt < testItemCount; cnt++)
				{
					var i = new Item();
					allItems.Add(i);
				}
			}
		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public void BlockingCollectionProcessor_Instance_ThrowsOnNullAction()
		{
			int testItemCount = 100;

			var allItems = new List<Item>(testItemCount);
			var collection = new System.Collections.Concurrent.BlockingCollection<Item>();
			using (var processor = new BlockingCollectionProcessor<Item>(collection, 4, null, ExpectedThreadLifetime.Short))
			{
				for (int cnt = 0; cnt < testItemCount; cnt++)
				{
					var i = new Item();
					allItems.Add(i);
					collection.Add(i);
				}

				collection.CompleteAdding();
				processor.WaitForCompletion();
				Assert.AreEqual(0, collection.Count);
				Assert.AreEqual(testItemCount, allItems.Count);
				Assert.IsFalse((from i in allItems where !i.Processed select i).Any());
				Assert.IsTrue(collection.IsCompleted);
			}
		}

		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		[TestMethod]
		public void BlockingCollectionProcessor_Instance_ThrowsOnZeroThreads()
		{
			int testItemCount = 100;

			var allItems = new List<Item>(testItemCount);
			var collection = new System.Collections.Concurrent.BlockingCollection<Item>();
			using (var processor = new BlockingCollectionProcessor<Item>(collection, 0, null, ExpectedThreadLifetime.Short))
			{
				for (int cnt = 0; cnt < testItemCount; cnt++)
				{
					var i = new Item();
					allItems.Add(i);
					collection.Add(i);
				}

				collection.CompleteAdding();
				processor.WaitForCompletion();
				Assert.AreEqual(0, collection.Count);
				Assert.AreEqual(testItemCount, allItems.Count);
				Assert.IsFalse((from i in allItems where !i.Processed select i).Any());
				Assert.IsTrue(collection.IsCompleted);
			}
		}

		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		[TestMethod]
		public void BlockingCollectionProcessor_Instance_ThrowsOnNegativeThreads()
		{
			int testItemCount = 100;

			var allItems = new List<Item>(testItemCount);
			var collection = new System.Collections.Concurrent.BlockingCollection<Item>();
			using (var processor = new BlockingCollectionProcessor<Item>(collection, -3, null, ExpectedThreadLifetime.Short))
			{
				for (int cnt = 0; cnt < testItemCount; cnt++)
				{
					var i = new Item();
					allItems.Add(i);
					collection.Add(i);
				}

				collection.CompleteAdding();
				processor.WaitForCompletion();
				Assert.AreEqual(0, collection.Count);
				Assert.AreEqual(testItemCount, allItems.Count);
				Assert.IsFalse((from i in allItems where !i.Processed select i).Any());
				Assert.IsTrue(collection.IsCompleted);
			}
		}


		[TestMethod]
		public void BlockingCollectionProcessor_Static_ProcessItems_ProcessesAllItems()
		{
			int testItemCount = 100;

			var allItems = new List<Item>(testItemCount);
			for (int cnt = 0; cnt < testItemCount; cnt++)
			{
				var i = new Item();
				allItems.Add(i);
			}

			BlockingCollectionProcessor<Item>.ProcessItems(allItems, 4,
				(item) =>
				{
					System.Threading.Thread.Sleep(100);
					item.Processed = true;
				},
				ExpectedThreadLifetime.Short
			);

			Assert.AreEqual(testItemCount, allItems.Count);
			Assert.IsFalse((from i in allItems where !i.Processed select i).Any());
		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public void BlockingCollectionProcessor_Static_ProcessItems_ThrowsOnNullCollection()
		{
			BlockingCollectionProcessor<Item>.ProcessItems(null, 4,
				(item) =>
				{
					System.Threading.Thread.Sleep(100);
					item.Processed = true;
				},
				ExpectedThreadLifetime.Short
			);
		}

		[ExpectedException(typeof(ArgumentNullException))]
		[TestMethod]
		public void BlockingCollectionProcessor_Static_ProcessItems_ThrowsOnNullAction()
		{
			var allItems = new List<Item>(100);

			BlockingCollectionProcessor<Item>.ProcessItems(allItems, 4, null, ExpectedThreadLifetime.Short);
		}

		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		[TestMethod]
		public void BlockingCollectionProcessor_Static_ProcessItems_ThrowsOnZeroThreads()
		{
			var allItems = new List<Item>(100);

			BlockingCollectionProcessor<Item>.ProcessItems(allItems, 0,
				(item) =>
				{
					System.Threading.Thread.Sleep(100);
					item.Processed = true;
				},
				ExpectedThreadLifetime.Short
			);
		}

		[ExpectedException(typeof(ArgumentOutOfRangeException))]
		[TestMethod]
		public void BlockingCollectionProcessor_Static_ProcessItems_ThrowsOnNegativeThreads()
		{
			var allItems = new List<Item>(100);

			BlockingCollectionProcessor<Item>.ProcessItems(allItems, -2,
				(item) =>
				{
					System.Threading.Thread.Sleep(100);
					item.Processed = true;
				},
				ExpectedThreadLifetime.Short
			);
		}

	}

	public class Item
	{
		public bool Processed { get; set; }
	}
}
