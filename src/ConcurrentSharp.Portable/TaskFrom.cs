using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentSharp
{
	/// <summary>
	/// Provides utility methods for creating <see cref="System.Threading.Tasks"/> from various sources or common patterns.
	/// </summary>
	public static class TaskFrom
	{

		#region From Event

		/// <summary>
		/// Creates a task that completes when an event is raised.
		/// </summary>
		/// <typeparam name="TSource">The type of object that publishes the event.</typeparam>
		/// <param name="source">An instance of <typeparamref name="TSource"/> that will raise the event.</param>
		/// <param name="eventName">The name of the event to wait for.</param>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that completes when the specified event is raised.</returns>
		public static Task @Event<TSource>(TSource source, string eventName)
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (String.IsNullOrWhiteSpace(eventName)) throw new ArgumentException("Event name cannot be null, empty or whitespace.", nameof(eventName));

			var declaredEvent = typeof(TSource).GetEvent(eventName);
			if (declaredEvent == null) throw new ArgumentException("Event not found.", nameof(eventName));

			var tcs = new TaskCompletionSource<EventArgs>();
			EventHandler handler = null;
			handler = (EventHandler)
			(
				(sender, e) =>
				{
					declaredEvent.RemoveEventHandler(source, handler);
					tcs.TrySetResult(e);
				}
			);

			declaredEvent.AddEventHandler(source, handler);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a task that completes when an event is raised, the result of the task is the event arguments passed to the event handler.
		/// </summary>
		/// <typeparam name="TSource">The type of object that publishes the event.</typeparam>
		/// <typeparam name="TEventArgs">The type of event arguments used by the event.</typeparam>
		/// <param name="source">An instance of <typeparamref name="TSource"/> that will raise the event.</param>
		/// <param name="eventName">The name of the event to wait for.</param>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that completes when the specified event is raised.</returns>
		public static Task<TEventArgs> @Event<TSource, TEventArgs>(TSource source, string eventName) where TEventArgs : EventArgs
		{
			if (source == null) throw new ArgumentNullException(nameof(source));
			if (String.IsNullOrWhiteSpace(eventName)) throw new ArgumentException("Event name cannot be null, empty or whitespace.", nameof(eventName));

			var declaredEvent = typeof(TSource).GetEvent(eventName);
			if (declaredEvent == null) throw new ArgumentException("Event not found.", nameof(eventName));

			var tcs = new TaskCompletionSource<TEventArgs>();
			EventHandler<TEventArgs> handler = null;
			handler = (EventHandler<TEventArgs>)
			(
				(sender, e) =>
				{
					declaredEvent.RemoveEventHandler(source, handler);
					tcs.TrySetResult(e);
				}
			);

			declaredEvent.AddEventHandler(source, handler);

			return tcs.Task;
		}

		#endregion

		#region FromAsyncCallback

		#region Functions

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="TReturn">The type of value returned from the EndAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="beginAsync"/> or <paramref name="endAsync"/> is null.</exception>
		public static Task<TReturn> FromAsyncCallback<TReturn>(Func<AsyncCallback, object, IAsyncResult> beginAsync, Func<IAsyncResult, TReturn> endAsync)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<TReturn>();

			beginAsync
			(
				(ar) =>
				{
					try
					{
						tcs.TrySetResult(endAsync(ar));
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				},
				null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="TReturn">The type of value returned from the EndAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <returns>The return value of the paired EndAsync method.</returns>
		public static Task<TReturn> FromAsyncCallback<T1, TReturn>(Func<T1, AsyncCallback, object, IAsyncResult> beginAsync, Func<IAsyncResult, TReturn> endAsync, T1 arg1)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<TReturn>();

			beginAsync
			(
				arg1,
				(ar) =>
				{
					try
					{
						tcs.TrySetResult(endAsync(ar));
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				},
				null);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="TReturn">The type of value returned from the EndAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <returns>The return value of the paired EndAsync method.</returns>
		public static Task<TReturn> FromAsyncCallback<T1, T2, TReturn>(Func<T1, T2, AsyncCallback, object, IAsyncResult> beginAsync, Func<IAsyncResult, TReturn> endAsync, T1 arg1, T2 arg2)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<TReturn>();

			beginAsync
			(
				arg1, arg2,
				(ar) =>
				{
					try
					{
						tcs.TrySetResult(endAsync(ar));
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="TReturn">The type of value returned from the EndAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		public static Task<TReturn> FromAsyncCallback<T1, T2, T3, TReturn>(Func<T1, T2, T3, AsyncCallback, object, IAsyncResult> beginAsync, Func<IAsyncResult, TReturn> endAsync, T1 arg1, T2 arg2, T3 arg3)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<TReturn>();

			beginAsync
			(
				arg1, arg2, arg3,
				(ar) =>
				{
					try
					{
						tcs.TrySetResult(endAsync(ar));
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="TReturn">The type of value returned from the EndAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		public static Task<TReturn> FromAsyncCallback<T1, T2, T3, T4, TReturn>(Func<T1, T2, T3, T4, AsyncCallback, object, IAsyncResult> beginAsync, Func<IAsyncResult, TReturn> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<TReturn>();

			beginAsync
			(
				arg1, arg2, arg3, arg4,
				(ar) =>
				{
					try
					{
						tcs.TrySetResult(endAsync(ar));
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="TReturn">The type of value returned from the EndAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <returns>The return value of the paired EndAsync method.</returns>
		public static Task<TReturn> FromAsyncCallback<T1, T2, T3, T4, T5, TReturn>(Func<T1, T2, T3, T4, T5, AsyncCallback, object, IAsyncResult> beginAsync, Func<IAsyncResult, TReturn> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<TReturn>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, 
				(ar) =>
				{
					try
					{
						tcs.TrySetResult(endAsync(ar));
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T6">The type of the sixth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="TReturn">The type of value returned from the EndAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <param name="arg6">The sixth argument to the BeginAsync method.</param>
		/// <returns>The return value of the paired EndAsync method.</returns>
		public static Task<TReturn> FromAsyncCallback<T1, T2, T3, T4, T5, T6, TReturn>(Func<T1, T2, T3, T4, T5, T6, AsyncCallback, object, IAsyncResult> beginAsync, Func<IAsyncResult, TReturn> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<TReturn>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, arg6, 
				(ar) =>
				{
					try
					{
						tcs.TrySetResult(endAsync(ar));
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T6">The type of the sixth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T7">The type of the seventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="TReturn">The type of value returned from the EndAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <param name="arg6">The sixth argument to the BeginAsync method.</param>
		/// <param name="arg7">The seventh argument to the BeginAsync method.</param>
		public static Task<TReturn> FromAsyncCallback<T1, T2, T3, T4, T5, T6, T7, TReturn>(Func<T1, T2, T3, T4, T5, T6, T7, AsyncCallback, object, IAsyncResult> beginAsync, Func<IAsyncResult, TReturn> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<TReturn>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, arg6, arg7, 
				(ar) =>
				{
					try
					{
						tcs.TrySetResult(endAsync(ar));
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T6">The type of the sixth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T7">The type of the seventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T8">The type of the eighth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="TReturn">The type of value returned from the EndAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <param name="arg6">The sixth argument to the BeginAsync method.</param>
		/// <param name="arg7">The seventh argument to the BeginAsync method.</param>
		/// <param name="arg8">The eighth argument to the BeginAsync method.</param>
		/// <returns>The return value of the paired EndAsync method.</returns>
		public static Task<TReturn> FromAsyncCallback<T1, T2, T3, T4, T5, T6, T7, T8, TReturn>(Func<T1, T2, T3, T4, T5, T6, T7, T8, AsyncCallback, object, IAsyncResult> beginAsync, Func<IAsyncResult, TReturn> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<TReturn>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8,
				(ar) =>
				{
				try
				{
					tcs.TrySetResult(endAsync(ar));
				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T6">The type of the sixth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T7">The type of the seventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T8">The type of the eighth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T9">The type of the ninth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="TReturn">The type of value returned from the EndAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <param name="arg6">The sixth argument to the BeginAsync method.</param>
		/// <param name="arg7">The seventh argument to the BeginAsync method.</param>
		/// <param name="arg8">The eighth argument to the BeginAsync method.</param>
		/// <param name="arg9">The ninth argument to the BeginAsync method.</param>
		/// <returns>The return value of the paired EndAsync method.</returns>
		public static Task<TReturn> FromAsyncCallback<T1, T2, T3, T4, T5, T6, T7, T8, T9, TReturn>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, AsyncCallback, object, IAsyncResult> beginAsync, Func<IAsyncResult, TReturn> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<TReturn>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, 
				(ar) =>
				{
					try
					{
						tcs.TrySetResult(endAsync(ar));
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T6">The type of the sixth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T7">The type of the seventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T8">The type of the eighth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T9">The type of the ninth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T10">The type of the tenth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="TReturn">The type of value returned from the EndAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <param name="arg6">The sixth argument to the BeginAsync method.</param>
		/// <param name="arg7">The seventh argument to the BeginAsync method.</param>
		/// <param name="arg8">The eighth argument to the BeginAsync method.</param>
		/// <param name="arg9">The ninth argument to the BeginAsync method.</param>
		/// <param name="arg10">The tenth argument to the BeginAsync method.</param>
		/// <returns>The return value of the paired EndAsync method.</returns>
		public static Task<TReturn> FromAsyncCallback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TReturn>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, AsyncCallback, object, IAsyncResult> beginAsync, Func<IAsyncResult, TReturn> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<TReturn>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10,
				(ar) =>
				{
					try
					{
						tcs.TrySetResult(endAsync(ar));
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T6">The type of the sixth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T7">The type of the seventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T8">The type of the eighth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T9">The type of the ninth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T10">The type of the tenth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T11">The type of the eleventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="TReturn">The type of value returned from the EndAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <param name="arg6">The sixth argument to the BeginAsync method.</param>
		/// <param name="arg7">The seventh argument to the BeginAsync method.</param>
		/// <param name="arg8">The eighth argument to the BeginAsync method.</param>
		/// <param name="arg9">The ninth argument to the BeginAsync method.</param>
		/// <param name="arg10">The tenth argument to the BeginAsync method.</param>
		/// <param name="arg11">The eleventh argument to the BeginAsync method.</param>
		/// <returns>The return value of the paired EndAsync method.</returns>
		public static Task<TReturn> FromAsyncCallback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TReturn>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, AsyncCallback, object, IAsyncResult> beginAsync, Func<IAsyncResult, TReturn> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<TReturn>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11,
				(ar) =>
				{
					try
					{
						tcs.TrySetResult(endAsync(ar));
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T6">The type of the sixth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T7">The type of the seventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T8">The type of the eighth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T9">The type of the ninth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T10">The type of the tenth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T11">The type of the eleventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T12">The type of the twelfth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="TReturn">The type of value returned from the EndAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <param name="arg6">The sixth argument to the BeginAsync method.</param>
		/// <param name="arg7">The seventh argument to the BeginAsync method.</param>
		/// <param name="arg8">The eighth argument to the BeginAsync method.</param>
		/// <param name="arg9">The ninth argument to the BeginAsync method.</param>
		/// <param name="arg10">The tenth argument to the BeginAsync method.</param>
		/// <param name="arg11">The eleventh argument to the BeginAsync method.</param>
		/// <param name="arg12">The twelfth argument to the BeginAsync method.</param>
		/// <returns>The return value of the paired EndAsync method.</returns>
		public static Task<TReturn> FromAsyncCallback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TReturn>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, AsyncCallback, object, IAsyncResult> beginAsync, Func<IAsyncResult, TReturn> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<TReturn>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, 
				(ar) =>
				{
					try
					{
						tcs.TrySetResult(endAsync(ar));
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T6">The type of the sixth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T7">The type of the seventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T8">The type of the eighth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T9">The type of the ninth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T10">The type of the tenth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T11">The type of the eleventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T12">The type of the twelfth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T13">The type of the thirteenth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="TReturn">The type of value returned from the EndAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <param name="arg6">The sixth argument to the BeginAsync method.</param>
		/// <param name="arg7">The seventh argument to the BeginAsync method.</param>
		/// <param name="arg8">The eighth argument to the BeginAsync method.</param>
		/// <param name="arg9">The ninth argument to the BeginAsync method.</param>
		/// <param name="arg10">The tenth argument to the BeginAsync method.</param>
		/// <param name="arg11">The eleventh argument to the BeginAsync method.</param>
		/// <param name="arg12">The twelfth argument to the BeginAsync method.</param>
		/// <param name="arg13">The thirteenth argument to the BeginAsync method.</param>
		/// <returns>The return value of the paired EndAsync method.</returns>
		public static Task<TReturn> FromAsyncCallback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TReturn>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, AsyncCallback, object, IAsyncResult> beginAsync, Func<IAsyncResult, TReturn> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<TReturn>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13,
				(ar) =>
				{
					try
					{
						tcs.TrySetResult(endAsync(ar));
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T6">The type of the sixth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T7">The type of the seventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T8">The type of the eighth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T9">The type of the ninth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T10">The type of the tenth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T11">The type of the eleventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T12">The type of the twelfth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T13">The type of the thirteenth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T14">The type of the fourteenth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="TReturn">The type of value returned from the EndAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <param name="arg6">The sixth argument to the BeginAsync method.</param>
		/// <param name="arg7">The seventh argument to the BeginAsync method.</param>
		/// <param name="arg8">The eighth argument to the BeginAsync method.</param>
		/// <param name="arg9">The ninth argument to the BeginAsync method.</param>
		/// <param name="arg10">The tenth argument to the BeginAsync method.</param>
		/// <param name="arg11">The eleventh argument to the BeginAsync method.</param>
		/// <param name="arg12">The twelfth argument to the BeginAsync method.</param>
		/// <param name="arg13">The thirteenth argument to the BeginAsync method.</param>
		/// <param name="arg14">The fourteenth argument to the BeginAsync method.</param>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the async operation, the task reuslt is the return value of the paired EndAsync method.</returns>
		public static Task<TReturn> FromAsyncCallback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TReturn>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14,  AsyncCallback, object, IAsyncResult> beginAsync, Func<IAsyncResult, TReturn> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<TReturn>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, 
				(ar) =>
				{
					try
					{
						tcs.TrySetResult(endAsync(ar));
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		#endregion

		#region Actions

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents the async operation.</returns>
		public static Task FromAsyncCallback<T1>(Func<T1, AsyncCallback, object, IAsyncResult> beginAsync, Action<IAsyncResult> endAsync, T1 arg1)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<object>();

			beginAsync
			(
				arg1,
				(ar) =>
				{
					try
					{
						endAsync(ar); tcs.SetResult(null);
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents the async operation.</returns>
		public static Task FromAsyncCallback<T1, T2>(Func<T1, T2, AsyncCallback, object, IAsyncResult> beginAsync, Action<IAsyncResult> endAsync, T1 arg1, T2 arg2)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<object>();

			beginAsync
			(
				arg1, arg2, 
				(ar) =>
				{
					try
					{
						endAsync(ar); tcs.SetResult(null);
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents the async operation.</returns>
		public static Task FromAsyncCallback<T1, T2, T3>(Func<T1, T2, T3, AsyncCallback, object, IAsyncResult> beginAsync, Action<IAsyncResult> endAsync, T1 arg1, T2 arg2, T3 arg3)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<object>();

			beginAsync
			(
				arg1, arg2, arg3,
				(ar) =>
				{
					try
					{
						endAsync(ar); tcs.SetResult(null);
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents the async operation.</returns>
		public static Task FromAsyncCallback<T1, T2, T3, T4>(Func<T1, T2, T3, T4, AsyncCallback, object, IAsyncResult> beginAsync, Action<IAsyncResult> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<object>();

			beginAsync
			(
				arg1, arg2, arg3, arg4,
				(ar) =>
				{
					try
					{
						endAsync(ar); tcs.SetResult(null);
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents the async operation.</returns>
		public static Task FromAsyncCallback<T1, T2, T3, T4, T5>(Func<T1, T2, T3, T4, T5, AsyncCallback, object, IAsyncResult> beginAsync, Action<IAsyncResult> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<object>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, 
				(ar) =>
				{
					try
					{
						endAsync(ar); tcs.SetResult(null);
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T6">The type of the sixth argument to the BeginAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <param name="arg6">The sixth argument to the BeginAsync method.</param>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents the async operation.</returns>
		public static Task FromAsyncCallback<T1, T2, T3, T4, T5, T6>(Func<T1, T2, T3, T4, T5, T6, AsyncCallback, object, IAsyncResult> beginAsync, Action<IAsyncResult> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<object>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, arg6, 

				(ar) =>
				{
					try
					{
						endAsync(ar); tcs.SetResult(null);
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T6">The type of the sixth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T7">The type of the seventh argument to the BeginAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <param name="arg6">The sixth argument to the BeginAsync method.</param>
		/// <param name="arg7">The seventh argument to the BeginAsync method.</param>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents the async operation.</returns>
		public static Task FromAsyncCallback<T1, T2, T3, T4, T5, T6, T7>(Func<T1, T2, T3, T4, T5, T6, T7, AsyncCallback, object, IAsyncResult> beginAsync, Action<IAsyncResult> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<object>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, arg6, arg7,

				(ar) =>
				{
					try
					{
						endAsync(ar); tcs.SetResult(null);
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T6">The type of the sixth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T7">The type of the seventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T8">The type of the eighth argument to the BeginAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <param name="arg6">The sixth argument to the BeginAsync method.</param>
		/// <param name="arg7">The seventh argument to the BeginAsync method.</param>
		/// <param name="arg8">The eighth argument to the BeginAsync method.</param>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents the async operation.</returns>
		public static Task FromAsyncCallback<T1, T2, T3, T4, T5, T6, T7, T8>(Func<T1, T2, T3, T4, T5, T6, T7, T8, AsyncCallback, object, IAsyncResult> beginAsync, Action<IAsyncResult> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<object>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, 

				(ar) =>
				{
					try
					{
						endAsync(ar); tcs.SetResult(null);
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T6">The type of the sixth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T7">The type of the seventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T8">The type of the eighth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T9">The type of the ninth argument to the BeginAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <param name="arg6">The sixth argument to the BeginAsync method.</param>
		/// <param name="arg7">The seventh argument to the BeginAsync method.</param>
		/// <param name="arg8">The eighth argument to the BeginAsync method.</param>
		/// <param name="arg9">The ninth argument to the BeginAsync method.</param>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents the async operation.</returns>
		public static Task FromAsyncCallback<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, AsyncCallback, object, IAsyncResult> beginAsync, Action<IAsyncResult> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<object>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, 

				(ar) =>
				{
					try
					{
						endAsync(ar); tcs.SetResult(null);
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T6">The type of the sixth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T7">The type of the seventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T8">The type of the eighth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T9">The type of the ninth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T10">The type of the tenth argument to the BeginAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <param name="arg6">The sixth argument to the BeginAsync method.</param>
		/// <param name="arg7">The seventh argument to the BeginAsync method.</param>
		/// <param name="arg8">The eighth argument to the BeginAsync method.</param>
		/// <param name="arg9">The ninth argument to the BeginAsync method.</param>
		/// <param name="arg10">The tenth argument to the BeginAsync method.</param>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents the async operation.</returns>
		public static Task FromAsyncCallback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, AsyncCallback, object, IAsyncResult> beginAsync, Action<IAsyncResult> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<object>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, 

				(ar) =>
				{
					try
					{
						endAsync(ar); tcs.SetResult(null);
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T6">The type of the sixth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T7">The type of the seventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T8">The type of the eighth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T9">The type of the ninth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T10">The type of the tenth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T11">The type of the eleventh argument to the BeginAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <param name="arg6">The sixth argument to the BeginAsync method.</param>
		/// <param name="arg7">The seventh argument to the BeginAsync method.</param>
		/// <param name="arg8">The eighth argument to the BeginAsync method.</param>
		/// <param name="arg9">The ninth argument to the BeginAsync method.</param>
		/// <param name="arg10">The tenth argument to the BeginAsync method.</param>
		/// <param name="arg11">The eleventh argument to the BeginAsync method.</param>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents the async operation.</returns>
		public static Task FromAsyncCallback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, AsyncCallback, object, IAsyncResult> beginAsync, Action<IAsyncResult> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<object>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, 

				(ar) =>
				{
					try
					{
						endAsync(ar); tcs.SetResult(null);
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T6">The type of the sixth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T7">The type of the seventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T8">The type of the eighth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T9">The type of the ninth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T10">The type of the tenth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T11">The type of the eleventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T12">The type of the twelfth argument to the BeginAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <param name="arg6">The sixth argument to the BeginAsync method.</param>
		/// <param name="arg7">The seventh argument to the BeginAsync method.</param>
		/// <param name="arg8">The eighth argument to the BeginAsync method.</param>
		/// <param name="arg9">The ninth argument to the BeginAsync method.</param>
		/// <param name="arg10">The tenth argument to the BeginAsync method.</param>
		/// <param name="arg11">The eleventh argument to the BeginAsync method.</param>
		/// <param name="arg12">The twelfth argument to the BeginAsync method.</param>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents the async operation.</returns>
		public static Task FromAsyncCallback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, AsyncCallback, object, IAsyncResult> beginAsync, Action<IAsyncResult> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<object>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, 

				(ar) =>
				{
					try
					{
						endAsync(ar); tcs.SetResult(null);
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T6">The type of the sixth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T7">The type of the seventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T8">The type of the eighth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T9">The type of the ninth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T10">The type of the tenth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T11">The type of the eleventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T12">The type of the twelfth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T13">The type of the thirteenth argument to the BeginAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <param name="arg6">The sixth argument to the BeginAsync method.</param>
		/// <param name="arg7">The seventh argument to the BeginAsync method.</param>
		/// <param name="arg8">The eighth argument to the BeginAsync method.</param>
		/// <param name="arg9">The ninth argument to the BeginAsync method.</param>
		/// <param name="arg10">The tenth argument to the BeginAsync method.</param>
		/// <param name="arg11">The eleventh argument to the BeginAsync method.</param>
		/// <param name="arg12">The twelfth argument to the BeginAsync method.</param>
		/// <param name="arg13">The thirteenth argument to the BeginAsync method.</param>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents the async operation.</returns>
		public static Task FromAsyncCallback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, AsyncCallback, object, IAsyncResult> beginAsync, Action<IAsyncResult> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<object>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, 

				(ar) =>
				{
					try
					{
						endAsync(ar); tcs.SetResult(null);
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		/// <summary>
		/// Creates a <see cref="System.Threading.Tasks.Task"/> from a BeginAsync.. EndAsync pair of methods.
		/// </summary>
		/// <typeparam name="T1">The type of the first argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T2">The type of the second argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T3">The type of the third argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T4">The type of the fourth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T5">The type of the fifth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T6">The type of the sixth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T7">The type of the seventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T8">The type of the eighth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T9">The type of the ninth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T10">The type of the tenth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T11">The type of the eleventh argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T12">The type of the twelfth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T13">The type of the thirteenth argument to the BeginAsync method.</typeparam>
		/// <typeparam name="T14">The type of the fourteenth argument to the BeginAsync method.</typeparam>
		/// <param name="beginAsync">A delegate or lambda reprsenting the BeginAsync method.</param>
		/// <param name="endAsync">A delegate or lambda representing the paired EndAsync method.</param>
		/// <param name="arg1">The first argument to the BeginAsync method.</param>
		/// <param name="arg2">The second argument to the BeginAsync method.</param>
		/// <param name="arg3">The third argument to the BeginAsync method.</param>
		/// <param name="arg4">The fourth argument to the BeginAsync method.</param>
		/// <param name="arg5">The fifth argument to the BeginAsync method.</param>
		/// <param name="arg6">The sixth argument to the BeginAsync method.</param>
		/// <param name="arg7">The seventh argument to the BeginAsync method.</param>
		/// <param name="arg8">The eighth argument to the BeginAsync method.</param>
		/// <param name="arg9">The ninth argument to the BeginAsync method.</param>
		/// <param name="arg10">The tenth argument to the BeginAsync method.</param>
		/// <param name="arg11">The eleventh argument to the BeginAsync method.</param>
		/// <param name="arg12">The twelfth argument to the BeginAsync method.</param>
		/// <param name="arg13">The thirteenth argument to the BeginAsync method.</param>
		/// <param name="arg14">The fourteenth argument to the BeginAsync method.</param>
		/// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents the async operation.</returns>
		public static Task FromAsyncCallback<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, AsyncCallback, object, IAsyncResult> beginAsync, Action<IAsyncResult> endAsync, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14)
		{
			if (beginAsync == null) throw new ArgumentNullException(nameof(beginAsync));
			if (endAsync == null) throw new ArgumentNullException(nameof(endAsync));

			var tcs = new TaskCompletionSource<object>();

			beginAsync
			(
				arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, 
				(ar) =>
				{
					try
					{
						endAsync(ar); tcs.SetResult(null);
					}
					catch (Exception ex)
					{
						tcs.TrySetException(ex);
					}
				}
				, null
			);

			return tcs.Task;
		}

		#endregion

		#endregion

	}
}