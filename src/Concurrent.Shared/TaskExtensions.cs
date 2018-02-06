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
	}
}