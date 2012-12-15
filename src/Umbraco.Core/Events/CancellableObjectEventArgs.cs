using System.Security.Permissions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Events
{
	/// <summary>
	/// Event args for a strongly typed object that can support cancellation
	/// </summary>
	/// <typeparam name="T"></typeparam>
	[HostProtection(SecurityAction.LinkDemand, SharedState = true)]
	public class CancellableObjectEventArgs<T> : CancellableEventArgs
	{

		public CancellableObjectEventArgs(T entity, bool canCancel)
			: base(canCancel)
		{
			Entity = entity;
		}

		public CancellableObjectEventArgs(T entity)
			: this(entity, true)
		{
		}


		public T Entity { get; private set; }

	}
}