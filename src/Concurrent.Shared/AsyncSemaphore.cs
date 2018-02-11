using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrentSharp
{
	/// <summary>
	/// Provides synchronisation between multiple threads, restricting concurrent operations to a maximum limit.
	/// </summary>
	public sealed class AsyncSemaphore : IReleasable, IDisposable
	{
		private System.Threading.Semaphore _Semaphore;
		private System.Collections.Generic.Queue<CancellableTaskSource<IDisposable>> _Queue;

		/// <summary>
		/// Constructs a new <see cref="AsyncSemaphore"/>.
		/// </summary>
		/// <param name="initialCount">The initial number of operations available.</param>
		/// <param name="maximumCount">The maximum number of operations to allow concurrently.</param>
		public AsyncSemaphore(int initialCount, int maximumCount) : this(new System.Threading.Semaphore(initialCount, maximumCount))
		{
		}

		/// <summary>
		/// Constructs a new <see cref="AsyncSemaphore"/> using an existing (synchronous) <see cref="System.Threading.Semaphore"/> as the synchronisation primitive.
		/// </summary>
		/// <param name="semaphore">The <see cref="System.Threading.Semaphore"/> to use for synchronisation.</param>
		private AsyncSemaphore(System.Threading.Semaphore semaphore)
		{
			if (semaphore == null) throw new ArgumentNullException(nameof(semaphore));
			_Queue = new System.Collections.Generic.Queue<CancellableTaskSource<IDisposable>>();
			_Semaphore = semaphore;
		}

		/// <summary>
		/// Asynchronously attempts to obtain a lease from the semaphore, returns a task whose result releases the lease when it is disposed.
		/// </summary>
		/// <returns>A task whose result is disposable, and releases the lease when disposed.</returns>
		public Task<IDisposable> WaitAsync()
		{
			return WaitAsync(CancellationToken.None);
		}

		/// <summary>
		/// Asynchronously attempts to obtain a lease from the semaphore, returns a task whose result releases the lease when it is disposed.
		/// </summary>
		/// <param name="cancellationToken">The cancellation token used to cancel waiting for a lease from the semaphore.</param>
		/// <returns>A task whose result is disposable, and releases the lease when disposed.</returns>
		public Task<IDisposable> WaitAsync(CancellationToken cancellationToken)
		{
			TaskCompletionSource<IDisposable> tcs = null;
			CancellableTaskSource<IDisposable> ctcs = null;
			Task<IDisposable> retVal = null;
			try { }
			finally
			{
#if SUPPORTS_THREADABORTEXCEPTION
				try
				{
#endif
					lock (_Queue)
					{
						if (_Semaphore.WaitOne(0))
#if BCL_ASYNC
							retVal = TaskEx.FromResult<IDisposable>(new ReleaseToken(this));
#else
							retVal = Task.FromResult<IDisposable>(new ReleaseToken(this));
#endif
						else
						{
							tcs = new TaskCompletionSource<IDisposable>();
							retVal = tcs.Task;
							ctcs = new CancellableTaskSource<IDisposable>(tcs, cancellationToken);
							_Queue.Enqueue(ctcs);
						}
					}
#if SUPPORTS_THREADABORTEXCEPTION
				}
				catch (ThreadAbortException)
				{
					if (tcs == null)
						retVal.Result.Dispose();
					else
					{
						ctcs.Cancel();
						ctcs.Dispose();
					}
				}
#endif
			}

			return retVal;
		}

#pragma warning disable 1574
		/// <summary>
		/// Synchronously attempts to obtain a lease from the semaphore, returns a task whose result releases the lease when it is disposed.
		/// </summary>
		/// <remarks>
		/// <para>On platforms that support <see cref="System.Runtime.ExceptionServices.ExceptionDispatchInfo"/> any exceptions thrown by the task will be unwrapped, on other platforms the <see cref="AggregateException"/> will be thown and must be manually handled.</para>
		/// </remarks>
		/// <returns>A task whose result is disposable, and releases the lease when disposed.</returns>
#pragma warning restore 1574
		public IDisposable Wait()
		{
			return Wait(CancellationToken.None);
		}

#pragma warning disable 1574
		/// <summary>
		/// Synchronously attempts to obtain a lease from the semaphore, returns a task whose result releases the lease when it is disposed.
		/// </summary>
		/// <remarks>
		/// <para>On platforms that support <see cref="System.Runtime.ExceptionServices.ExceptionDispatchInfo"/> any exceptions thrown by the task will be unwrapped, on other platforms the <see cref="AggregateException"/> will be thown and must be manually handled.</para>
		/// </remarks>
		/// <param name="cancellationToken">The cancellation token used to cancel waiting for a lease from the semaphore.</param>
		/// <returns>A task whose result is disposable, and releases the lease when disposed.</returns>
#pragma warning restore 1574
		public IDisposable Wait(CancellationToken cancellationToken)
		{
#if SUPPORTS_EXCEPTIONSERVICES
			return WaitAsync(cancellationToken).WaitWithUnwrappedException();
#else
			return WaitAsync(cancellationToken).Result;
#endif
		}

		void IReleasable.Release()
		{
			lock (_Queue)
			{
				CancellableTaskSource<IDisposable> next = null;
				while (_Queue.Count > 0 && (next = _Queue.Dequeue()) != null)
				{
					//If the task is already cancelled etc. then this will return false
					//and we move to the next requested lock.
					if (next.TaskCompletionSource.TrySetResult(new ReleaseToken(this))) break;
				}
			}
		}

		/// <summary>
		/// Disposes this instance and all internal resources.
		/// </summary>
		public void Dispose()
		{
			_Semaphore?.Dispose();
			_Semaphore = null;
		}
	}
}