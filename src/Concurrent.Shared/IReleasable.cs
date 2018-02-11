using System;
using System.Collections.Generic;
using System.Text;

namespace ConcurrentSharp
{
	internal interface IReleasable
	{
		void Release();
	}
}