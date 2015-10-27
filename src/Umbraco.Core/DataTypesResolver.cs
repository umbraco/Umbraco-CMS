using System;
using System.Collections.Generic;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;
using umbraco.interfaces;

namespace Umbraco.Core
{
	/// <summary>
	/// A resolver to return all IDataType objects
	/// </summary>
    [Obsolete("IDataType is obsolete and is no longer used, it will be removed from the codebase in future versions")]
	internal sealed class DataTypesResolver : LegacyTransientObjectsResolver<DataTypesResolver, IDataType>
	{
	    /// <summary>
	    /// Constructor
	    /// </summary>
	    /// <param name="serviceProvider"></param>
	    /// <param name="logger"></param>
	    /// <param name="dataTypes"></param>		
	    internal DataTypesResolver(IServiceProvider serviceProvider, ILogger logger, Func<IEnumerable<Type>> dataTypes)
            : base(serviceProvider, logger, dataTypes)
		{

		}

		/// <summary>
		/// Gets the <see cref="ICacheRefresher"/> implementations.
		/// </summary>
		public IEnumerable<IDataType> DataTypes
		{
			get
			{
				EnsureIsInitialized();
				return Values;
			}
		}

		protected override Guid GetUniqueIdentifier(IDataType obj)
		{
			return obj.Id;
		}
	}
}