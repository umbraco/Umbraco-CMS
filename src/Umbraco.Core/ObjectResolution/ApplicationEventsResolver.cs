using System;
using System.Collections.Generic;
using System.Linq;
using umbraco.interfaces;

namespace Umbraco.Core.ObjectResolution
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

		protected override bool SupportsClear
		{
			get { return false; }
		}		

		protected override bool SupportsInsert
		{
			get { return false; }			
		}

		protected override bool SupportsRemove
		{
			get { return false; }
		}

	}
}