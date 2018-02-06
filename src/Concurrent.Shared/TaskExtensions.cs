using System;
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

	}
}