using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentSharp
{
	/// <summary>
	/// Instances of the <see cref="Throttle"/> class can be used to restrict the number of simultaneous operations taking place using the same throttle.
	/// </summary>
	/// <remarks>
	/// <para>To use a <see cref="Throttle"/> create an instance specifying the maximum number of concurrent operations you want to occur. 
	/// Then have each operation call <see cref="Throttle.Enter"/> when it begins and capture the returned token. When the operation completes, call <see cref="IDisposable.Dispose()"/> on the returned token.
	/// The Throttle will ensure no more than the specified number of operations run at the same time.</para>
	/// </remarks>
	/// <example>
	/// <code>
	/// var throttle = new Throttle(4); //No more than 4 simultaneous operations.
	/// 
	/// // Start 100 calls to a 'DoWork' function.
	/// for (int cnt = 0; cnt &lt; 100; cnt++)
	/// {
	///		System.Threading.ThreadPool.QueueUserWorkItem
	///		(
	///			(reserved) => 
	///			{
	///				using (var token = throttle.Enter()) //Obtain a token from the throttle instance to restrict the concurrent jobs
	///				{
	///					DoWork(throttle);
	///				} // Token will be disposed here, allowing another job to run if one is waiting.
	///			}
	///		);
	/// }
	/// </code>
	/// </example>
	public sealed class Throttle : IDisposable
	{

		private System.Threading.Semaphore _Semaphore;
		private object _Synchroniser;

		/// <summary>
		/// Constructs a new <see cref="Throttle"/> instance that allows up to the specified number of concurrent operations.
		/// </summary>
		/// <param name="maxConcurrency">The maximum number of allowed concurrent operations.</param>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="maxConcurrency"/> is less than or equal to zero.</exception>
		public Throttle(int maxConcurrency)
		{
			if (maxConcurrency <= 0) throw new ArgumentOutOfRangeException(nameof(maxConcurrency));

			_Synchroniser = new object();
			_Semaphore = new System.Threading.Semaphore(maxConcurrency, maxConcurrency);
		}

		/// <summary>
		/// Creates and returns an object implementing <see cref="IDisposable"/> that can be used to notify this throttle that a job has completed. If there are already the maximum number of 
		/// operations running using this throttle, then this method will block the calling thread until another job completes.
		/// </summary>
		/// <returns>A <see cref="ThrottleToken"/> that can be used to indicate a throttled operation has completed and another operation can begin.</returns>
		/// <exception cref="ObjectDisposedException">Thrown if this instance is already disposed, or is disposed while attempting/waiting to pass the throttle.</exception>
		/// <example>
		/// <code>
		/// public void DoWork()
		/// {
		///		// Assumes there is there is a _Throttle field set
		///		// to a valid instance of the Throttle class.
		///		using (var token = _Throttle.Enter())
		///		{
		///			// Your actual work goes here.
		///		}
		/// }
		/// </code>
		/// </example>
		public IDisposable Enter()
		{
			ThrowIfDisposed();

			while (!_Semaphore?.WaitOne(500) ?? false)
			{
				if (_Semaphore == null) throw new ObjectDisposedException(nameof(Throttle));
			}

			return new ThrottleToken(_Semaphore);
		}

		/// <summary>
		/// A convenience method that executes the <paramref name="action"/> operation using the throttle, eliminating manual management of the throttle token.
		/// </summary>
		/// <param name="action">The job to execute via the throttle.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null.</exception>
		/// <example>
		/// <code>
		/// var throttle = new Throttle(4); //No more than 4 simultaneous operations.
		/// 
		/// // Start 100 calls to a 'DoWork' function.
		/// for (int cnt = 0; cnt &lt; 100; cnt++)
		/// {
		///		System.Threading.ThreadPool.QueueUserWorkItem
		///		(
		///			(reserved) =>
		///			{
		///				throttle.Execute(() => DoWork()); // Execute the action via the throttle's Execute method.
		///			}
		/// }
		/// </code>
		/// </example>
		public void Execute(Action action)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			using (var token = Enter())
			{
				action();
			}
		}

		/// <summary>
		/// A convenience method that executes the <paramref name="action"/> operation using the throttle, eliminating manual management of the throttle token.
		/// </summary>
		/// <typeparam name="T">The type of argument passed to the action.</typeparam>
		/// <param name="action">The job to execute via the throttle.</param>
		/// <param name="arg">The value to pass to <paramref name="action"/> when it is executed.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="action"/> is null.</exception>
		public void Execute<T>(Action<T> action, T arg)
		{
			if (action == null) throw new ArgumentNullException(nameof(action));

			using (var token = Enter())
			{
				action(arg);
			}
		}

		/// <summary>
		/// A convenience method that executes the <paramref name="func"/> operation using the throttle and returning it's result while eliminating manual management of the throttle token.
		/// </summary>
		/// <typeparam name="TResult">The type of value returned by <paramref name="func"/>.</typeparam>
		/// <param name="func">The function to execute via the throttle.</param>
		/// <returns>The value returned by the <paramref name="func"/> parameter.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> is null.</exception>
		/// <example>
		/// <code>
		/// var throttle = new Throttle(4); //No more than 4 simultaneous operations.
		/// 
		/// var results = new List*lt;int&gt;();
		/// // Start 100 calls to a 'DoWork' function.
		/// for (int cnt = 0; cnt &lt; 100; cnt++)
		/// {
		///		System.Threading.ThreadPool.QueueUserWorkItem
		///		(
		///			(reserved) =>
		///			{
		///				var result = throttle.Execute(() => DoWork()); 
		///				lock (results)
		///				{
		///					results.Add(result);
		///				}
		///			}
		/// }
		/// </code>
		/// </example>
		public TResult Execute<TResult>(Func<TResult> func)
		{
			if (func == null) throw new ArgumentNullException(nameof(func));

			using (var token = Enter())
			{
				return func();
			}
		}

		/// <summary>
		/// A convenience method that executes the <paramref name="func"/> operation using the throttle and returning it's result while eliminating manual management of the throttle token.
		/// </summary>
		/// <typeparam name="T">The type of argument passed to the function.</typeparam>
		/// <typeparam name="TResult">The type of value returned by <paramref name="func"/>.</typeparam>
		/// <param name="func">The function to execute via the throttle.</param>
		/// <param name="arg">The value to pass to <paramref name="func"/> when it is executed.</param>
		/// <returns>The value returned by the <paramref name="func"/> parameter.</returns>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="func"/> is null.</exception>
		public TResult Execute<T, TResult>(Func<T, TResult> func, T arg)
		{
			if (func == null) throw new ArgumentNullException(nameof(func));

			using (var token = Enter())
			{
				return func(arg);
			}
		}

		/// <summary>
		/// Executes <paramref name="asyncAction"/> over all items in <paramref name="items"/>, using the throttle to restrict the maximum number of parallel executing tasks.
		/// </summary>
		/// <typeparam name="T">The type of value in <paramref name="items"/>.</typeparam>
		/// <param name="items">The items to process.</param>
		/// <param name="asyncAction">A function that returns a task and takes a value of {T}.</param>
		/// <returns>Returns a <see cref="System.Threading.Tasks.Task"/> that represents completion processing all items in <paramref name="items"/>.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="items"/> or <paramref name="asyncAction"/> is null.</exception>
		public void ExecuteAll<T>(IEnumerable<T> items, Action<T> asyncAction)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

			using (var countdownEvent = new System.Threading.CountdownEvent(0))
			{
				foreach (var item in items)
				{
					if (countdownEvent.IsSet)
						countdownEvent.Reset();

					countdownEvent.AddCount();

					_Semaphore.WaitOne();
					System.Threading.ThreadPool.QueueUserWorkItem
					(
						(i) =>
						{
							try
							{
								asyncAction((T)i);
							}
							finally
							{
								_Semaphore.Release();
								countdownEvent.Signal();
							}
						}, item
					);
				}

				countdownEvent.Wait();
			}
		}

		/// <summary>
		/// Executes <paramref name="asyncAction"/> over all items in <paramref name="items"/>, using the throttle to restrict the maximum number of parallel executing tasks.
		/// </summary>
		/// <typeparam name="TArg">The type of value in <paramref name="items"/>.</typeparam>
		/// <typeparam name="TResult"></typeparam>
		/// <param name="items">The items to process.</param>
		/// <param name="asyncAction">A function that returns a task of {TResult} and takes a value of {TArg}.</param>
		/// <returns>Returns a task whose result is an enumerable of completed tasks.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown if <paramref name="items"/> or <paramref name="asyncAction"/> is null.</exception>
		public IEnumerable<TResult> ExecuteAll<TArg, TResult>(IEnumerable<TArg> items, Func<TArg, TResult> asyncAction)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (asyncAction == null) throw new ArgumentNullException(nameof(asyncAction));

			using (var countdownEvent = new System.Threading.CountdownEvent(0))
			{
				var retVal = new List<TResult>();
				foreach (var item in items)
				{
					if (countdownEvent.IsSet)
						countdownEvent.Reset();

					countdownEvent.AddCount();

					_Semaphore.WaitOne();
					System.Threading.ThreadPool.QueueUserWorkItem
					(
						(i) =>
						{
							try
							{
								var result = asyncAction((TArg)i);
								lock (retVal)
								{
									retVal.Add(result);
								}
							}
							finally
							{
								_Semaphore.Release();
								countdownEvent.Signal();
							}
						}, item
					);
				}

				countdownEvent.Wait();

				return retVal;
			}
		}

		private void ThrowIfDisposed()
		{
			if (_Semaphore == null) throw new ObjectDisposedException(nameof(Throttle));
		}

		/// <summary>
		/// Disposes this throttle and all internal resources. 
		/// </summary>
		/// <remarks>
		/// <para>Will cause any currently pending or future calls to <see cref="Enter"/> to throw a <see cref="ObjectDisposedException"/>.</para>
		/// <para>Does not stop any pending operations.</para>
		/// </remarks>
		public void Dispose()
		{
			var semaphore = _Semaphore;
			_Semaphore = null;
			if (semaphore != null)
			{
				lock (_Synchroniser)
				{
					semaphore.Dispose();
				}
			}
		}
	}

}
