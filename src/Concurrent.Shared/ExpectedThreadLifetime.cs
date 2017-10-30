using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConcurrentSharp
{
	/// <summary>
	/// An enumeration that specifies the (relative) expected lifetime of a thread or background operation, helping to decide how the thread should be created.
	/// </summary>
	public enum ExpectedThreadLifetime
	{
		/// <summary>
		/// The thread is expected to run for a short period of time, or the operation is expected to run one and then end, in a relatively short period of time.
		/// </summary>
		Short = 0,
		/// <summary>
		/// The thread or operation is expected to run for a long period of time, possibly for the lifetime of the process.
		/// </summary>
		Long
	}
}