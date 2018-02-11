using System;
using System.Collections.Generic;
using System.Text;

namespace ConcurrentSharp
{
	/// <summary>
	/// An object returned by <see cref="Throttle.Enter"/> that is used to notify the parent <see cref="Throttle"/> instance the job owning this token has completed.
	/// </summary>
	internal sealed class ThrottleToken : IDisposable
	{
		private System.Threading.Semaphore _Semaphore;
		private object _Synchroniser;

		internal ThrottleToken(System.Threading.Semaphore semaphore)
		{
			_Synchroniser = new object();
			_Semaphore = semaphore;
		}

		/// <summary>
		/// "Releases the throttle". Notifies the parent <see cref="Throttle"/> instance the job owning this token has completed and another job can be started.
		/// </summary>
		public void Dispose()
		{
			var semaphore = _Semaphore;
			_Semaphore = null;
			if (semaphore != null)
			{
				lock (_Synchroniser)
				{
					try
					{
						//Note: Dispose does not seem to throw on subsequent calls
						//on desktop framework, but can't guarantee that for other
						//frameworks or future versions, so handle the likely error
						//anyway.
						semaphore.Release();
					}
					catch (ObjectDisposedException)
					{
						// Parent throttle was disposed, 
						// but this is not a fatal error for the
						// completing operation calling this method.
					}
				}
			}
		}
	}
}