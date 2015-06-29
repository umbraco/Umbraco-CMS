using System;
using System.Collections.Generic;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Persistence.Migrations
{
	/// <summary>
	/// A resolver to return all IMigrations
	/// </summary>
	internal class MigrationResolver : LazyManyObjectsResolverBase<MigrationResolver, IMigration>
	{
	    
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="migrations"></param>
		/// <remarks>
		/// Use transient objects as we don't want these as singletons and take up memory that is not required
		/// </remarks>
		public MigrationResolver(Func<IEnumerable<Type>> migrations)
			: base(migrations, ObjectLifetimeScope.Transient)
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