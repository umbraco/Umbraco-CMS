using System;
using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Persistence.Migrations
{
	/// <summary>
	/// A resolver to return all IMigrations
	/// </summary>
	internal class MigrationResolver : ManyObjectsResolverBase<MigrationResolver, IMigration>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="surfaceControllers"></param>
		/// <remarks>
		/// Use transient objects as we don't want these as singletons and take up memory that is not required
		/// </remarks>
		public MigrationResolver(IEnumerable<Type> surfaceControllers)
			: base(surfaceControllers, ObjectLifetimeScope.Transient)
		{			
		}

		/// <summary>
		/// Gets the migrations
		/// </summary>
		public IEnumerable<IMigration> Migrations
		{
			get { return Values; }
		}		
	}
}