using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace ConcurrentSharp
{
	/// <summary>
	/// Instances of <see cref="BlockingCollectionProcessor{T}"/> are used to manage threads for processing <see cref="System.Collections.Concurrent.BlockingCollection{T}"/> instances.
	/// </summary>
	/// <remarks>
	/// <para>This class also exposes a static method called <see cref="ProcessItems(IEnumerable{T}, int, Action{T}, ExpectedThreadLifetime)"/> which can be used to process all the items in any <see cref="IEnumerable{T}"/>,
	/// encapsulating creating and managing the blocking collection as well as processing the items.</para>
	/// <para>To use this class, create or obtain a reference to the blocking collection you want to process. Create a new instance of <see cref="BlockingCollectionProcessor{T}"/> passing it the collection and other 
	/// configuration information. Then add items to the blocking collection as required, they will automatically be processed. When you've finished and will no longer be adding any more items to the collection 
	/// call <see cref="WaitForCompletion()"/> and then <see cref="Dispose()"/> on the processor. Any items already in the blocking collection when the processor is created will also be processed.</para>
	/// </remarks>
	/// <typeparam name="T">The type of value stored in the <see cref="BlockingCollection{T}"/> used by the processor.</typeparam>
	/// <example>
	/// <code>
	/// //Create a collection to contain items that need processing
	/// var myCollection = new BlockingCollection&lt;Customer&gt;();
	/// 
	/// // Create a processor that only processes up to 4 items at a time, is short lived and uses a method called
	/// // 'DoWork' to process items.
	/// var myProcessor = new BlockingCollectionProcessor&lt;Customer&gt;(collection, 4, DoWork, ExpectedthreadLifetime.Short);
	/// 
	/// foreach (var customer in customerRepository.GetAll())
	/// {
	///		myProcessor.Add(customer);
	/// }
	/// 
	/// myProcessor.WaitForCompletion();
	/// myProcessor.Dispose();
	/// 
	/// </code>
	/// </example>
	public sealed class BlockingCollectionProcessor<T> : IDisposable
	{

		#region Fields

		private readonly BlockingCollection<T> _Collection;
		private readonly Action<T> _ProcessItem;
		private System.Threading.Semaphore _CompleteSemaphore;
		private int _MaxThreads;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructs a new instance of the <see cref="BlockingCollectionProcessor{T}"/> class.
		/// </summary>
		/// <param name="collection">The <see cref="BlockingCollection{T}"/> to process items from.</param>
		/// <param name="numberOfProcessingThreads">The number of processing threads to create.</param>
		/// <param name="processItem">The action used to process individual items in <paramref name="collection"/>.</param>
		/// <param name="threadLifetime">A value from the <see cref="ExpectedThreadLifetime"/> enum specifying whether this processor (and it's child threads) is expected to be long or short lived.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="collection"/> or <paramref name="processItem"/> is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown if <paramref name="numberOfProcessingThreads"/> is less than or equal to zero.</exception>
		public BlockingCollectionProcessor(BlockingCollection<T> collection, int numberOfProcessingThreads, Action<T> processItem, ExpectedThreadLifetime threadLifetime)
		{
			if (numberOfProcessingThreads <= 0) throw new ArgumentOutOfRangeException(nameof(numberOfProcessingThreads));

			_Collection = collection ?? throw new ArgumentNullException(nameof(collection));
			_ProcessItem = processItem ?? throw new ArgumentNullException(nameof(processItem));

			_Collection = collection;
			_ProcessItem = processItem;
			_CompleteSemaphore = new System.Threading.Semaphore(numberOfProcessingThreads, numberOfProcessingThreads);
			_MaxThreads = numberOfProcessingThreads;

			for (int cnt = 0; cnt < numberOfProcessingThreads; cnt++)
			{
				StartThread(threadLifetime);
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Waits for the collection to be marked as adding complete, and for all items in the collection to be processed, before returning control to the calling method unless a timeout occurs first.
		/// </summary>
		/// <param name="timeout">The maximum time to wait before timing out.</param>
		/// <remarks>Returns a boolean indciating whether the processing completed (true), or if a timeout occurred (false).</remarks>
		/// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
		public bool WaitForCompletion(TimeSpan timeout)
		{
			if (_CompleteSemaphore == null) throw new ObjectDisposedException(nameof(BlockingCollectionProcessor<T>));

			var startTime = DateTime.Now;
			for (int cnt = 0; cnt < _MaxThreads; cnt++)
			{
				var thisTimeout = Convert.ToInt32(timeout.Milliseconds - DateTime.Now.Subtract(startTime).TotalMilliseconds);
				if (thisTimeout <= 0) return false;

				if (!_CompleteSemaphore.WaitOne(thisTimeout))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Waits for the collection to be marked as adding complete, and afor all items in the collection to be processed, before returning control to the calling method.
		/// </summary>
		/// <exception cref="ObjectDisposedException">Thrown if this instance has been disposed.</exception>
		public void WaitForCompletion()
		{
			if (_CompleteSemaphore == null) throw new ObjectDisposedException(nameof(BlockingCollectionProcessor<T>));

			for (int cnt = 0; cnt < _MaxThreads; cnt++)
			{
				_CompleteSemaphore.WaitOne();
			}
		}

		#endregion

		#region IDisposable Members

		/// <summary>
		/// Disposes this instance and all internal resources.
		/// </summary>
		public void Dispose()
		{
			if (!_Collection.IsAddingCompleted)
				_Collection?.CompleteAdding();

			if (!_Collection.IsCompleted)
				_Collection?.Dispose();

			_CompleteSemaphore?.Dispose();
			_CompleteSemaphore = null;
		}

		#endregion

		#region Private Methods

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "threadLifetime", Justification = "Usage depends on conditional compilation")]
		private void StartThread(ExpectedThreadLifetime threadLifetime)
		{
			try
			{
				_CompleteSemaphore.WaitOne();

#if SUPPORTS_THREAD
			if (threadLifetime == ExpectedThreadLifetime.Long)
			{
					var thread = new System.Threading.Thread(this.ProcessItems)
					{
						IsBackground = true
					};
					thread.Start();
				return;
			}
#endif

#if SUPPORTS_THREADPOOL
				System.Threading.ThreadPool.QueueUserWorkItem((reserved) => ProcessItems());
#else
			System.Threading.Tasks.Task.Factory.StartNew
			(
				(reserved) => ProcessItems(),
				System.Threading.CancellationToken.None,
				(threadLifetime == ExpectedThreadLifetime.Long ? System.Threading.Tasks.TaskCreationOptions.LongRunning : System.Threading.Tasks.TaskCreationOptions.None)
			).Ignore();
#endif
			}
			catch
			{
				_CompleteSemaphore.Release();
				throw;
			}
		}

		private void ProcessItems()
		{
			try
			{
				T item = default(T);
				while (!_Collection.IsCompleted)
				{
					if (_Collection.TryTake(out item))
					{
						_ProcessItem(item);
					}
				}
			}
			finally
			{
				_CompleteSemaphore.Release();
			}
		}

		#endregion

		#region Static Implementation

		/// <summary>
		/// Processes all ites in <paramref name="items"/> and returns control to the calling method when all items have completed processing.
		/// </summary>
		/// <param name="items">An <see cref="IEnumerable{T}"/> containing the items to be processed.</param>
		/// <param name="numberOfProcessingThreads">The maximum number of items to process at once.</param>
		/// <param name="processItem">The action used to process the items in <paramref name="items"/>.</param>
		/// <param name="threadLifetime">A value from the <see cref="ExpectedThreadLifetime"/> enum specifying whether this processor (and it's child threads) is expected to be long or short lived.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="items"/> or <paramref name="processItem"/> is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="numberOfProcessingThreads"/> is less than or equal to zero.</exception>
		/// <example>
		/// <code>
		/// // Process all items returned by customerRepository.GetAll(), processing no more than 4 at at time, using a method called DoWork to do the actual processing.
		/// BlockingCollectionProcessor&lt;Customer&gt;.ProcessItems(customerRepository.GetAll(), 4, DoWork, ExpectedThreadLifetime.Short);
		/// </code>
		/// </example>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes")]
		public static void ProcessItems(IEnumerable<T> items, int numberOfProcessingThreads, Action<T> processItem, ExpectedThreadLifetime threadLifetime)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (numberOfProcessingThreads <= 0) throw new ArgumentOutOfRangeException(nameof(numberOfProcessingThreads));
			if (processItem == null) throw new ArgumentNullException(nameof(processItem));

			using (var collection = new BlockingCollection<T>())
			{
				using (var processor = new BlockingCollectionProcessor<T>(collection, numberOfProcessingThreads, processItem, threadLifetime))
				{
					foreach (var item in items)
					{
						collection.Add(item);
					}
					collection.CompleteAdding();
					processor.WaitForCompletion();
				}
			}
		}

		#endregion

	}
}