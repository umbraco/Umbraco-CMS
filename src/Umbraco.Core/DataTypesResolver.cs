using System;
using System.Collections.Generic;
using umbraco.interfaces;

namespace Umbraco.Core
{
	/// <summary>
	/// A resolver to return all IDataType objects
	/// </summary>
	internal sealed class DataTypesResolver : LegacyTransientObjectsResolver<IDataType>
	{

		#region Singleton

		private static readonly DataTypesResolver Instance = new DataTypesResolver(PluginTypeResolver.Current.ResolveDataTypes());

		public static DataTypesResolver Current
		{
			get { return Instance; }
		}
		#endregion

		#region Constructors
		static DataTypesResolver() { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="dataTypes"></param>		
		internal DataTypesResolver(IEnumerable<Type> dataTypes)
			: base(dataTypes)
		{

		}
		#endregion

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