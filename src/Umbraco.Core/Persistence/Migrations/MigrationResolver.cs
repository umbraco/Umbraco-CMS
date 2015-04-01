using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence.SqlSyntax;

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
	    /// <param name="logger"></param>
	    /// <param name="migrations"></param>
	    /// <remarks>
	    /// Use transient objects as we don't want these as singletons and take up memory that is not required
	    /// </remarks>
	    public MigrationResolver(ILogger logger, Func<IEnumerable<Type>> migrations)
            : base(new MigrationServiceProvider(), logger, migrations, ObjectLifetimeScope.Transient)
		{			
		}

		/// <summary>
		/// Gets the migrations
		/// </summary>
		public IEnumerable<IMigration> Migrations
		{
			get { return Values; }
		}


        /// <summary>
        /// This will ctor the IMigration instances
        /// </summary>
        /// <remarks>
        /// This is like a super crappy DI - in v8 we have real DI
        /// </remarks>
	    private class MigrationServiceProvider : IServiceProvider
	    {
	        public object GetService(Type serviceType)
	        {
                var normalArgs = new[] {typeof (ISqlSyntaxProvider), typeof (ILogger)};
	            var found = serviceType.GetConstructor(normalArgs);
	            if (found != null)
	                return found.Invoke(new object[] {ApplicationContext.Current.DatabaseContext.SqlSyntax, LoggerResolver.Current.Logger});
	            return Activator.CreateInstance(serviceType);
	        }
	    }
	}
}