using System;
using System.Collections.Generic;
using System.Text;

namespace ConcurrentSharp
{
	/// <summary>
	/// A token that when disposed releases a lock or other associated synchronisation primitive.
	/// </summary>
	internal sealed class ReleaseToken : IDisposable
	{
		private bool _IsDisposed;
		private IReleasable _Releasable;

		internal ReleaseToken(IReleasable releasable)
		{
			_Releasable = releasable;
		}

		/// <summary>
		/// Disposes this instance and releases the associated lock. Only releases the lock once, subsequent calls will effectively be a no-op.
		/// </summary>
		public void Dispose()
		{
			try { }
			finally // Prevent threadaborts messing up release.
			{
				lock (_Releasable)
				{
					if (!_IsDisposed)
					{
						_Releasable.Release();
						_IsDisposed = true;
					}
				}
			}
		}
	}
}
