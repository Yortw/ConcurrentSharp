using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ConcurrentSharp
{
	public class AsyncEventHandle : EventWaitHandle
	{

		private EventWaitHandle _EventWaitHandle;

		public AsyncEventHandle(EventWaitHandle eventWaitHandle)
		{
			if (eventWaitHandle == null) throw new ArgumentNullException(nameof(eventWaitHandle));

			_EventWaitHandle = eventWaitHandle;
		}


	}
}