using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrentSharp
{

	/// <summary>
	///  Combines a <see cref="TaskCompletionSource{TResult}"/> and a <see cref="CancellationToken"/> so the task can be cancelled via the token, while ensuring all resources are correctly cleaned up regardless of how the task completes.
	/// </summary>
	/// <typeparam name="T">The type of value returned by the task from the <see cref="TaskCompletionSource{TResult}"/>.</typeparam>
	public sealed class CancellableTaskSource<T> : IDisposable
	{

		#region Fields

		private System.Threading.Tasks.TaskCompletionSource<T> _Tcs;
		private System.Threading.CancellationTokenSource _CancellationTokenSource;
		private System.Threading.CancellationToken _CancellationToken;
		private System.Threading.CancellationTokenRegistration _CancellationRegistration;

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a new <see cref="CancellationTokenSource"/>, with a new <see cref="TaskCompletionSource{TResult}"/> and a new <see cref="System.Threading.CancellationToken"/>.
		/// </summary>
		public CancellableTaskSource() : this(new TaskCompletionSource<T>())
		{

		}

		/// <summary>
		/// Creates a new <see cref="CancellationTokenSource"/> using the provided <paramref name="taskCompletionSource"/> and a new <see cref="System.Threading.CancellationToken"/>.
		/// </summary>
		/// <param name="taskCompletionSource">The <see cref="System.Threading.Tasks.TaskCompletionSource{TResult}"/> to associate with this instance.</param>
		public CancellableTaskSource(TaskCompletionSource<T> taskCompletionSource)
		{
			_Tcs = taskCompletionSource;
			_CancellationTokenSource = new System.Threading.CancellationTokenSource();
			_CancellationToken = _CancellationTokenSource.Token;

			Initialise();
		}

		/// <summary>
		/// Creates a new <see cref="CancellationTokenSource"/> using the provided <paramref name="taskCompletionSource"/> and a new <see cref="System.Threading.CancellationToken"/>.
		/// </summary>
		/// <param name="taskCompletionSource">The <see cref="System.Threading.Tasks.TaskCompletionSource{TResult}"/> to associate with this instance.</param>
		/// <param name="cancellationToken">A <see cref="System.Threading.CancellationToken"/> which when cancelled should cancel the <paramref name="taskCompletionSource"/> task.</param>
		public CancellableTaskSource(TaskCompletionSource<T> taskCompletionSource, System.Threading.CancellationToken cancellationToken)
		{
			_Tcs = taskCompletionSource;
			_CancellationToken = cancellationToken;

			Initialise();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Tries to cancel the task. If the task is already complete (cancelled, ran to completion or faulted) this is effectively a no-op.
		/// </summary>
		public void Cancel()
		{
			if (_CancellationTokenSource != null)
				_CancellationTokenSource.Cancel();
			else
				_Tcs.TrySetCanceled();
		}

		#endregion

		#region Public Properties

		/// <summary>
		/// Returns the underlying <see cref="TaskCompletionSource{TResult}"/> source.
		/// </summary>
		public TaskCompletionSource<T> TaskCompletionSource
		{
			get
			{
				return _Tcs;
			}
		}

		/// <summary>
		/// Returns the underlying <see cref="CancellationToken"/>.
		/// </summary>
		public CancellationToken CancellationToken
		{
			get { return _CancellationToken; }
		}

		#endregion

		#region Private Methods

		private void Initialise()
		{
			if (_Tcs.Task.IsCompleted) return;

			_CancellationRegistration = _CancellationToken.Register
			(
				() =>
				{
					_Tcs.TrySetCanceled();

					_CancellationTokenSource?.Dispose();
					_CancellationRegistration.Dispose();
				},
				false
			);

			_Tcs.Task.ContinueWith((pt) => _CancellationRegistration.Dispose(), TaskContinuationOptions.ExecuteSynchronously);
		}

		#endregion

		#region IDisposable

		/// <summary>
		/// Disposes this instance and all internal resources.
		/// </summary>
		public void Dispose()
		{
			_CancellationTokenSource?.Dispose();
			_CancellationRegistration.Dispose();
		}

		#endregion

	}
}