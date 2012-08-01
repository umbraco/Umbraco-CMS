using System;
using System.Collections.Generic;
using Umbraco.Core.Resolving;
using umbraco.interfaces;

namespace Umbraco.Core
{
	/// <summary>
	/// A resolver to return all IDataType objects
	/// </summary>
	internal sealed class DataTypesResolver : LegacyTransientObjectsResolver<DataTypesResolver, IDataType>
	{

	
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="dataTypes"></param>		
		internal DataTypesResolver(IEnumerable<Type> dataTypes)
			: base(dataTypes)
		{

		}

		/// <summary>
		/// Gets the <see cref="ICacheRefresher"/> implementations.
		/// </summary>
		public IEnumerable<IDataType> DataTypes
		{
			get
			{
				EnsureRefreshersList();
				return Values;
			}
		}

		protected override Guid GetUniqueIdentifier(IDataType obj)
		{
			return obj.Id;
		}
	}
}