using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.ObjectResolution;
using umbraco.interfaces;

namespace Umbraco.Web
{
	/// <summary>
	/// A resolver to return all IApplicationEvents objects
	/// </summary>
	internal sealed class ApplicationEventsResolver : ManyObjectsResolverBase<ApplicationEventsResolver, IApplicationStartupHandler>
	{

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="applicationEventHandlers"></param>		
		internal ApplicationEventsResolver(IEnumerable<Type> applicationEventHandlers)
			: base(applicationEventHandlers)
		{

		}

		/// <summary>
		/// Gets the <see cref="IApplicationEventHandler"/> implementations.
		/// </summary>
		public IEnumerable<IApplicationEventHandler> ApplicationEventHandlers
		{
			get { return Values.OfType<IApplicationEventHandler>(); }
		}

		public override void Clear()
		{
			throw new NotSupportedException("This class does not support this method");
		}

		public override void AddType(Type value)
		{
			throw new NotSupportedException("This class does not support this method");
		}

		public override void InsertType(int index, Type value)
		{
			throw new NotSupportedException("This class does not support this method");
		}

		public override void RemoveType(Type value)
		{
			throw new NotSupportedException("This class does not support this method");
		}

	}
}