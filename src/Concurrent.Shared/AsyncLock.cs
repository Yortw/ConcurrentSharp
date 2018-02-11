using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrentSharp
{
	/// <summary>
	/// Provides an awaitable (non-blocking) synchronisation primitve.
	/// </summary>
	/// <remarks>
	/// <para>Where you might use;</para>
	/// <example>
	/// <code>
	/// private object _Synchroniser = new object();
	/// ...
	/// lock (_Synchroniser)
	/// {
	///		// Your synchronised code (await not allowed)
	/// }
	/// </code>
	/// </example>
	/// <para>
	/// You would instead use (note, use of the await keyword within the using is critical for correct behaviour);
	/// </para>
	/// <example>
	/// <code>
	/// private AsyncLock _AsyncLock = new AsyncLock();
	/// ...
	/// using (var lock = await _AsyncLock())
	/// {
	///		// Your synchronised code, can use await
	/// }
	/// </code>
	/// </example>
	/// </remarks>
	public class AsyncLock : IReleasable
	{

		private object _Synchroniser;
		private bool _LockHeld;
		private System.Collections.Generic.Queue<CancellableTaskSource<IDisposable>> _LockQueue;

		/// <summary>
		/// Contructs a new <see cref="AsyncLock"/> instance.
		/// </summary>
		public AsyncLock()
		{
			_Synchroniser = new object();
			_LockQueue = new System.Collections.Generic.Queue<CancellableTaskSource<IDisposable>>();
		}

		/// <summary>
		/// Returns a task that completes when the lock is acquired. The result of the returned task is an <see cref="IDisposable"/> instance that releases the lock when disposed.
		/// </summary>
		/// <returns>A task whose result is an <see cref="IDisposable"/> used to release the lock.</returns>
		public Task<IDisposable> LockAsync()
		{
			return LockAsync(CancellationToken.None);
		}

		/// <summary>
		/// Returns a task that completes when the lock is acquired. The result of the returned task is an <see cref="IDisposable"/> instance that releases the lock when disposed.
		/// </summary>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel waiting for the lock.</param>
		/// <returns>A task whose result is an <see cref="IDisposable"/> used to release the lock.</returns>
		public Task<IDisposable> LockAsync(System.Threading.CancellationToken cancellationToken)
		{
			TaskCompletionSource<IDisposable> tcs = null;
			CancellableTaskSource<IDisposable> ctcs = null;
			Task<IDisposable> retVal = null;

#if SUPPORTS_THREADABORTEXCEPTION
			bool threadOwnsLock = false;

			try
			{
#endif
			try { }
				finally // Ensure a thread abort doesn't cause partial locking
				{
					lock (_Synchroniser)
					{
						if (!_LockHeld)
						{
#if BCL_ASYNC
							_LockHeld = true;
							retVal = TaskEx.FromResult<IDisposable>(new ReleaseToken(this));
#else
							threadOwnsLock = _LockHeld = true;
							retVal = Task.FromResult<IDisposable>(new ReleaseToken(this));
#endif
						}
						else
						{
							tcs = new TaskCompletionSource<IDisposable>();
							ctcs = new CancellableTaskSource<IDisposable>(tcs, cancellationToken);
							retVal = tcs.Task;
							_LockQueue.Enqueue(ctcs);
						}
					}
				}
#if SUPPORTS_THREADABORTEXCEPTION
			}
			catch (ThreadAbortException) 
			{
				//Our thread is being aborted so undo our lock/request
				if (threadOwnsLock)
					retVal.Result.Dispose();
				else // We didn't take the lock, cancel our request so we don't get it in the future.
				{
					ctcs.Cancel();
					ctcs.Dispose();
				}
			}
#endif

			return retVal;
		}

#pragma warning disable 1574
		/// <summary>
		/// Acquires the lock synchronously and returns the <see cref="IDisposable"/> token that releases the lock when disposed.
		/// </summary>
		/// <remarks>
		/// <para>On platforms that support <see cref="System.Runtime.ExceptionServices.ExceptionDispatchInfo"/> any exceptions thrown by the task will be unwrapped, on other platforms the <see cref="AggregateException"/> will be thown and must be manually handled.</para>
		/// </remarks>
		/// <returns>A object implementing <see cref="IDisposable"/> that releases the lock when the dispose method is called.</returns>
#pragma warning restore 1574
		public IDisposable Lock()
		{
#if SUPPORTS_EXCEPTIONSERVICES
			return LockAsync().WaitWithUnwrappedException();
#else
			return LockAsync().Result;
#endif
		}

		void IReleasable.Release()
		{
			lock (_Synchroniser)
			{
				if (!_LockHeld) return;

				CancellableTaskSource<IDisposable> next = null;
				while (_LockQueue.Count > 0 && (next = _LockQueue.Dequeue()) != null)
				{
					//If the task is already cancelled etc. then this will return false
					//and we move to the next requested lock.
					if (next.TaskCompletionSource.TrySetResult(new ReleaseToken(this))) break;
				}

				if (next == null)
					_LockHeld = false;
			}
		}

	}
}