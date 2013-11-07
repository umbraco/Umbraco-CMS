using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Web;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;

namespace umbraco
{
	/// <summary>
	/// Static helper methods
	/// </summary>
	public static partial class uQuery
	{
		/// <summary>
		/// Gets all datatypes sorted by a specified property name
		/// </summary>
		/// <returns>
		/// A sorted list of datatypes. Null if property name is wrong.
		/// </returns>
		public static IEnumerable<KeyValuePair<int, string>> GetAllDataTypes()
		{
			return GetAllDataTypes(true);
		}

		/// <summary>
		/// Gets all datatypes sorted by a specified property name
		/// </summary>
		/// <param name="sortByName">if set to <c>true</c> [sort by name].</param>
		/// <returns>
		/// A sorted list of datatypes. Null if property name is wrong.
		/// </returns>
		public static IEnumerable<KeyValuePair<int, string>> GetAllDataTypes(bool sortByName)
		{
			var datatypes = new Dictionary<int, string>();

            // Go through the values from the database and store them in the array.
            var dtos = ApplicationContext.Current.DatabaseContext.Database.Fetch<NodeDto>(
                "SELECT d.nodeId id, n.text FROM cmsDataType d, umbracoNode n WHERE d.nodeId = n.id");
            foreach (var dto in dtos)
            {
                if (!dto.NodeId.Equals(new Guid()) && !string.IsNullOrEmpty(dto.Text))
                {
                    datatypes.Add(dto.NodeId, dto.Text);
                }                
            }

			if (sortByName)
			{
				return datatypes.OrderBy(x => x.Value);
			}

			return datatypes.OrderBy(x => x.Key);
		}

		/// <summary>
		/// Gets the data type GUID.
		/// </summary>
		/// <param name="datatypeNodeId">The datatype node id.</param>
		/// <returns></returns>
		public static Guid GetBaseDataTypeGuid(int datatypeNodeId)
		{
			var dataTypes = DataTypeDefinition.GetAll();

			foreach (var dataTypeDefinition in dataTypes)
			{
				try
				{
					var dataType = dataTypeDefinition.DataType;
					if (dataType != null && dataType.DataTypeDefinitionId == datatypeNodeId)
					{
						return dataTypeDefinition.DataType.Id;
					}
				}
				catch (Exception ex)
				{
                    LogHelper.Error<UmbracoHelper>(string.Format("uQuery error in datatype definition {0}", dataTypeDefinition.Id), ex);
				}
			}

			return new Guid();
		}
	}
}