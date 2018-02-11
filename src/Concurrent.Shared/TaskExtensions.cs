using System;
#if SUPPORTS_EXCEPTIONSERVICES
using System.Runtime.ExceptionServices;
#endif
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrentSharp
{
	/// <summary>
	/// Extensions for <see cref="System.Threading.Tasks.Task"/> instances.
	/// </summary>
	public static class TaskExtensions
	{

		/// <summary>
		/// Used to avoid compiler warnings for unused tasks returned from functions.
		/// </summary>
		/// <param name="task">The task to ignore.</param>
		/// <example>
		/// <code>
		///		DoSomethingAsync().IgnoreTask();
		/// </code>
		/// </example>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "task", Scope = "member", Target = "Concurrent.TaskExtensions.#IgnoreTask(System.Threading.Tasks.Task)")]
		public static void Ignore(this Task task)
		{
		}

		#region TimeoutAfter Methods

		/// <summary>
		/// Waits for the specified task to complete, or a timeout to occur, whichever happens first.
		/// </summary>
		/// <typeparam name="T">The type of value returned by the task.</typeparam>
		/// <param name="task">The task to wait for.</param>
		/// <param name="timeoutMilliseconds">The maximum time in milliseconds to wait. A value of zero or less than -1 will cause an immediate timeout. Specify <see cref="System.Threading.Timeout.Infinite"/> or -1 to indicate no timeout.</param>
		/// <remarks>
		/// <para>If a timeout occurs <paramref name="task"/> will not be cancelled and will still run to completion.</para>
		/// </remarks>
		/// <returns>A value of type {T} returned by <paramref name="task"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="task"/> is null.</exception>
		/// <exception cref="System.Threading.Tasks.TaskCanceledException">Thrown if <paramref name="task"/> is cancelled.</exception>
		/// <exception cref="System.TimeoutException">Thrown if a timeout occurs before <paramref name="task"/> completes.</exception>
		public static async Task<T> TimeoutAfter<T>(this Task<T> task, int timeoutMilliseconds)
		{
			return await TimeoutAfter(task, timeoutMilliseconds, null);
		}

		/// <summary>
		/// Waits for the specified task to complete, or a timeout to occur, whichever happens first.
		/// </summary>
		/// <typeparam name="T">The type of value returned by the task.</typeparam>
		/// <param name="task">The task to wait for.</param>
		/// <param name="timeout">The maximum time to wait. A value of zero or less than -1 will cause an immediate timeout. Specify a timespan of -1 milliseconds to indicate no timeout.</param>
		/// <remarks>
		/// <para>If a timeout occurs <paramref name="task"/> will not be cancelled and will still run to completion.</para>
		/// </remarks>
		/// <returns>A value of type {T} returned by <paramref name="task"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="task"/> is null.</exception>
		/// <exception cref="System.Threading.Tasks.TaskCanceledException">Thrown if <paramref name="task"/> is cancelled.</exception>
		/// <exception cref="System.TimeoutException">Thrown if a timeout occurs before <paramref name="task"/> completes.</exception>
		public static async Task<T> TimeoutAfter<T>(this Task<T> task, TimeSpan timeout)
		{
			return await TimeoutAfter(task, timeout.Milliseconds, null);
		}

		/// <summary>
		/// Waits for the specified task to complete, or a timeout to occur, whichever happens first.
		/// </summary>
		/// <typeparam name="T">The type of value returned by the task.</typeparam>
		/// <param name="task">The task to wait for.</param>
		/// <param name="timeout">The maximum time to wait. A value of zero or less than -1 will cause an immediate timeout. Specify a timespan of -1 milliseconds to indicate no timeout.</param>
		/// <param name="taskCancellationSource">A <see cref="System.Threading.CancellationTokenSource"/> that provides the cancellation token for <paramref name="task"/>.</param>
		/// <remarks>
		/// <para>If a timeout occurs <paramref name="taskCancellationSource"/> will be cancelled thereby cancelling <paramref name="task"/> if it was created using the associated token.</para>
		/// </remarks>
		/// <returns>A value of type {T} returned by <paramref name="task"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="task"/> is null.</exception>
		/// <exception cref="System.Threading.Tasks.TaskCanceledException">Thrown if <paramref name="task"/> is cancelled.</exception>
		/// <exception cref="System.TimeoutException">Thrown if a timeout occurs before <paramref name="task"/> completes.</exception>
		public static async Task<T> TimeoutAfter<T>(this Task<T> task, TimeSpan timeout, CancellationTokenSource taskCancellationSource)
		{
			return await TimeoutAfter(task, timeout.Milliseconds, taskCancellationSource);
		}

		/// <summary>
		/// Waits for the specified task to complete, or a timeout to occur, whichever happens first.
		/// </summary>
		/// <typeparam name="T">The type of value returned by the task.</typeparam>
		/// <param name="task">The task to wait for.</param>
		/// <param name="timeoutMilliseconds">The maximum time in milliseconds to wait. A value of zero or less than -1 will cause an immediate timeout. Specify <see cref="System.Threading.Timeout.Infinite"/> or -1 to indicate no timeout.</param>
		/// <param name="taskCancellationSource">A <see cref="System.Threading.CancellationTokenSource"/> that provides the cancellation token for <paramref name="task"/>.</param>
		/// <remarks>
		/// <para>If a timeout occurs <paramref name="taskCancellationSource"/> will be cancelled thereby cancelling <paramref name="task"/> if it was created using the associated token.</para>
		/// </remarks>
		/// <returns>A value of type {T} returned by <paramref name="task"/>.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="task"/> is null.</exception>
		/// <exception cref="System.Threading.Tasks.TaskCanceledException">Thrown if <paramref name="task"/> is cancelled.</exception>
		/// <exception cref="System.TimeoutException">Thrown if a timeout occurs before <paramref name="task"/> completes.</exception>
		public static async Task<T> TimeoutAfter<T>(this Task<T> task, int timeoutMilliseconds, CancellationTokenSource taskCancellationSource)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));
			if (task.IsCompleted || timeoutMilliseconds == System.Threading.Timeout.Infinite) return await task.ConfigureAwait(false); //If task is completed or there is no timeout, just return result.

			if (timeoutMilliseconds > 0) // If there is no timeout skip waiting for task and cancel immediately
			{
				using (var cancelTaskCancellationSource = new CancellationTokenSource())
				{
#if BCL_ASYNC
					var cancelTask = TaskEx.Delay(timeoutMilliseconds, cancelTaskCancellationSource.Token);
					if (task == await TaskEx.WhenAny(task, cancelTask).ConfigureAwait(false))
#else
					var cancelTask = Task.Delay(timeoutMilliseconds, cancelTaskCancellationSource.Token);
					if (task == await Task.WhenAny(task, cancelTask).ConfigureAwait(false))
#endif
					{
						if (!cancelTaskCancellationSource.IsCancellationRequested)
							cancelTaskCancellationSource.Cancel();

						return await task.ConfigureAwait(false);
					}
				}
			}

			if (!(taskCancellationSource?.IsCancellationRequested ?? true))
				taskCancellationSource.Cancel();

			if (!task.IsCanceled && task.IsCompleted) // Task completed before/at same time as we timed out, so return it instead.
				return await task.ConfigureAwait(false);

			//If we got here, we timed out.
			throw new TimeoutException();
		}

		/// <summary>
		/// Waits for the specified task to complete, or a timeout to occur, whichever happens first.
		/// </summary>
		/// <param name="task">The task to wait for.</param>
		/// <param name="timeoutMilliseconds">The maximum time in milliseconds to wait. A value of zero or less than -1 will cause an immediate timeout. Specify <see cref="System.Threading.Timeout.Infinite"/> or -1 to indicate no timeout.</param>
		/// <remarks>
		/// <para>If a timeout occurs <paramref name="task"/> will not be cancelled and will still run to completion.</para>
		/// </remarks>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents either completion of <paramref name="task"/> or a timeout.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="task"/> is null.</exception>
		/// <exception cref="System.Threading.Tasks.TaskCanceledException">Thrown if <paramref name="task"/> is cancelled.</exception>
		/// <exception cref="System.TimeoutException">Thrown if a timeout occurs before <paramref name="task"/> completes.</exception>
		public static async Task TimeoutAfter(this Task task, int timeoutMilliseconds)
		{
			await TimeoutAfter(task, timeoutMilliseconds, null);
		}

		/// <summary>
		/// Waits for the specified task to complete, or a timeout to occur, whichever happens first.
		/// </summary>
		/// <param name="task">The task to wait for.</param>
		/// <param name="timeout">The maximum time to wait. A value of zero or less than -1 will cause an immediate timeout. Specify a timespan of -1 milliseconds to indicate no timeout.</param>
		/// <remarks>
		/// <para>If a timeout occurs <paramref name="task"/> will not be cancelled and will still run to completion.</para>
		/// </remarks>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents either completion of <paramref name="task"/> or a timeout.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="task"/> is null.</exception>
		/// <exception cref="System.Threading.Tasks.TaskCanceledException">Thrown if <paramref name="task"/> is cancelled.</exception>
		/// <exception cref="System.TimeoutException">Thrown if a timeout occurs before <paramref name="task"/> completes.</exception>
		public static async Task TimeoutAfter(this Task task, TimeSpan timeout)
		{
			await TimeoutAfter(task, timeout.Milliseconds, null);
		}

		/// <summary>
		/// Waits for the specified task to complete, or a timeout to occur, whichever happens first.
		/// </summary>
		/// <param name="task">The task to wait for.</param>
		/// <param name="timeout">The maximum time to wait. A value of zero or less than -1 will cause an immediate timeout. Specify a timespan of -1 milliseconds to indicate no timeout.</param>
		/// <param name="taskCancellationSource">A <see cref="System.Threading.CancellationTokenSource"/> that provides the cancellation token for <paramref name="task"/>.</param>
		/// <remarks>
		/// <para>If a timeout occurs <paramref name="taskCancellationSource"/> will be cancelled thereby cancelling <paramref name="task"/> if it was created using the associated token.</para>
		/// </remarks>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents either completion of <paramref name="task"/> or a timeout.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="task"/> is null.</exception>
		/// <exception cref="System.Threading.Tasks.TaskCanceledException">Thrown if <paramref name="task"/> is cancelled.</exception>
		/// <exception cref="System.TimeoutException">Thrown if a timeout occurs before <paramref name="task"/> completes.</exception>
		public static async Task TimeoutAfter(this Task task, TimeSpan timeout, CancellationTokenSource taskCancellationSource)
		{
			await TimeoutAfter(task, timeout.Milliseconds, taskCancellationSource);
		}

		/// <summary>
		/// Waits for the specified task to complete, or a timeout to occur, whichever happens first.
		/// </summary>
		/// <param name="task">The task to wait for.</param>
		/// <param name="timeoutMilliseconds">The maximum time in milliseconds to wait. A value of zero or less than -1 will cause an immediate timeout. Specify <see cref="System.Threading.Timeout.Infinite"/> or -1 to indicate no timeout.</param>
		/// <param name="taskCancellationSource">A <see cref="System.Threading.CancellationTokenSource"/> that provides the cancellation token for <paramref name="task"/>.</param>
		/// <remarks>
		/// <para>If a timeout occurs <paramref name="taskCancellationSource"/> will be cancelled thereby cancelling <paramref name="task"/> if it was created using the associated token.</para>
		/// </remarks>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents either completion of <paramref name="task"/> or a timeout.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="task"/> is null.</exception>
		/// <exception cref="System.Threading.Tasks.TaskCanceledException">Thrown if <paramref name="task"/> is cancelled.</exception>
		/// <exception cref="System.TimeoutException">Thrown if a timeout occurs before <paramref name="task"/> completes.</exception>
		public static async Task TimeoutAfter(this Task task, int timeoutMilliseconds, CancellationTokenSource taskCancellationSource)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));

			if (task.IsCompleted || timeoutMilliseconds == System.Threading.Timeout.Infinite)
			{
				await task.ConfigureAwait(false); //If task is completed or there is no timeout, just return result.
				return;
			}

			if (timeoutMilliseconds > 0) // If there is no timeout skip waiting for task and cancel immediately
			{
				using (var cancelTaskCancellationSource = new CancellationTokenSource())
				{
#if BCL_ASYNC
					var cancelTask = TaskEx.Delay(timeoutMilliseconds, cancelTaskCancellationSource.Token);
					if (task == await TaskEx.WhenAny(task, cancelTask).ConfigureAwait(false))
#else
					var cancelTask = Task.Delay(timeoutMilliseconds, cancelTaskCancellationSource.Token);
					if (task == await Task.WhenAny(task, cancelTask).ConfigureAwait(false))
#endif
					{
						if (!cancelTaskCancellationSource.IsCancellationRequested)
							cancelTaskCancellationSource.Cancel();

						await task.ConfigureAwait(false);
						return;
					}
				}
			}

			if (!(taskCancellationSource?.IsCancellationRequested ?? true))
				taskCancellationSource.Cancel();

			if (!task.IsCanceled && task.IsCompleted) // Task completed before/at same time as we timed out, so return it instead.
			{
				await task.ConfigureAwait(false);
				return;
			}

			//If we got here, we timed out.
			throw new TimeoutException();
		}

		#endregion

		#region WaitWithUnwrappedException

#if SUPPORTS_EXCEPTIONSERVICES

		/// <summary>
		/// Synchronously waits for <paramref name="task"/> to complete, and if an <see cref="AggregateException"/> is thrown the inner exception is unwrapped and re-thrown.
		/// </summary>
		/// <param name="task">The task to wait for.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="task"/> is null.</exception>
		public static void WaitWithUnwrappedException(this Task task)
		{
			WaitWithUnwrappedException(task, CancellationToken.None);
		}

		/// <summary>
		/// Synchronously waits for <paramref name="task"/> to complete, and if an <see cref="AggregateException"/> is thrown the inner exception is unwrapped and re-thrown.
		/// </summary>
		/// <param name="task">The task to wait for.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel waiting for the task to complete.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="task"/> is null.</exception>
		/// <exception cref="OperationCanceledException">Thrown if the wait is cancelled via the <paramref name="cancellationToken"/> before the task completes.</exception>
		public static void WaitWithUnwrappedException(this Task task, CancellationToken cancellationToken)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));

			try
			{
				task.Wait(cancellationToken);
			}
			catch (AggregateException aex)
			{
				ExceptionDispatchInfo.Capture(aex.InnerException).Throw();
			}
		}

		/// <summary>
		/// Synchronously waits for <paramref name="task"/> to complete, and if an <see cref="AggregateException"/> is thrown the inner exception is unwrapped and re-thrown.
		/// </summary>
		/// <param name="task">The task to wait for.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="task"/> is null.</exception>
		/// <returns>The result of the task (a value of type {T}).</returns>
		public static T WaitWithUnwrappedException<T>(this Task<T> task)
		{
			return WaitWithUnwrappedException<T>(task, CancellationToken.None);
		}

		/// <summary>
		/// Synchronously waits for <paramref name="task"/> to complete, and if an <see cref="AggregateException"/> is thrown the inner exception is unwrapped and re-thrown.
		/// </summary>
		/// <param name="task">The task to wait for.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel waiting for the task to complete.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="task"/> is null.</exception>
		/// <exception cref="OperationCanceledException">Thrown if the wait is cancelled via the <paramref name="cancellationToken"/> before the task completes.</exception>
		/// <returns>The result of the task (a value of type {T}).</returns>
		public static T WaitWithUnwrappedException<T>(this Task<T> task, CancellationToken cancellationToken)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));

			try
			{
				task.Wait(cancellationToken);
				return task.Result;
			}
			catch (AggregateException aex)
			{
				ExceptionDispatchInfo.Capture(aex.InnerException).Throw();
				return default(T); // Will never get here, but avoids compiler error.
			}
		}

#endif

		#endregion

		#region WaitExceptionless

		/// <summary>
		/// Synchronously waits for <paramref name="task"/> to complete and ignores any exceptions thrown.
		/// </summary>
		/// <param name="task">The task to wait for.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="task"/> is null.</exception>
		public static void WaitExceptionless(this Task task)
		{
			WaitExceptionless(task, CancellationToken.None);
		}

		/// <summary>
		/// Synchronously waits for <paramref name="task"/> to complete and ignores any exceptions thrown.
		/// </summary>
		/// <param name="task">The task to wait for.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel waiting for the task to complete.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="task"/> is null.</exception>
		/// <exception cref="OperationCanceledException">Thrown if the wait is cancelled via the <paramref name="cancellationToken"/> before the task completes.</exception>
		public static void WaitExceptionless(this Task task, CancellationToken cancellationToken)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));

			try
			{
				task.Wait(cancellationToken);
			}
			catch (AggregateException)
			{
			}
		}

		/// <summary>
		/// Synchronously waits for <paramref name="task"/> to complete and ignores any exceptions thrown.
		/// </summary>
		/// <remarks>
		/// <para>If any exception is thrown by the task, the result of the call is the default value of {T}.</para>
		/// </remarks>
		/// <param name="task">The task to wait for.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="task"/> is null.</exception>
		/// <returns>The result of the task (a value of type {T}).</returns>
		public static T WaitExceptionless<T>(this Task<T> task)
		{
			return WaitExceptionless<T>(task, CancellationToken.None);
		}

		/// <summary>
		/// Synchronously waits for <paramref name="task"/> to complete and ignores any exceptions thrown.
		/// </summary>
		/// <remarks>
		/// <para>If any exception is thrown by the task, the result of the call is the default value of {T}.</para>
		/// </remarks>
		/// <param name="task">The task to wait for.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> used to cancel waiting for the task to complete.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="task"/> is null.</exception>
		/// <exception cref="OperationCanceledException">Thrown if the wait is cancelled via the <paramref name="cancellationToken"/> before the task completes.</exception>
		/// <returns>The result of the task (a value of type {T}).</returns>
		public static T WaitExceptionless<T>(this Task<T> task, CancellationToken cancellationToken)
		{
			if (task == null) throw new ArgumentNullException(nameof(task));

			try
			{
				task.Wait(cancellationToken);
				return task.Result;
			}
			catch (AggregateException)
			{
				return default(T); 
			}
		}

		#endregion

	}
}