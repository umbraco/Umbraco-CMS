using System.Collections;
using System.Collections.Generic;
using System.Linq;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.propertytype;
using umbraco.NodeFactory;
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
		/// Makes a new PreValue.
		/// </summary>
		/// <param name="dataTypeDefinitionId">The datatype definition id.</param>
		/// <param name="value">The value.</param>
		/// <param name="alias">The alias.</param>
		/// <param name="sortOrder">The sort order.</param>
		/// <returns>
		/// The inserted prevalue or null if the operation failed.
		/// </returns>
		public static PreValue MakeNewPreValue(int dataTypeDefinitionId, string value, string alias = "", int sortOrder = 0)
		{
            var dto = new DataTypePreValueDto
            {
                Alias = alias,
                DataTypeNodeId = dataTypeDefinitionId,
                SortOrder = sortOrder,
                Value = value,
            };

            var result = ApplicationContext.Current.DatabaseContext.Database.Insert(dto);

			if ((int)result > -1)
			{
				return uQuery.GetPreValues(dataTypeDefinitionId).Single(x => x.Value.Equals(value) && x.GetAlias().Equals(alias) && x.SortOrder == sortOrder);
			}

			return null;
		}

		/// <summary>
		/// Gets the prevalues for a specified datatype as a strongly typed list.
		/// </summary>
		/// <param name="dataTypeDefinitionId">The datatype definition id.</param>
		/// <returns>The list of PreValues.</returns>
		public static PreValue[] GetPreValues(int dataTypeDefinitionId)
		{
			var values = new List<PreValue>();

			foreach (var entry in PreValues.GetPreValues(dataTypeDefinitionId))
			{
				var prevalue = ((DictionaryEntry)entry).Value as PreValue;
				values.Add(prevalue);
			}

			return values.ToArray();
		}

		/// <summary>
		/// Gets the pre values.
		/// TODO: [OA] Document on Codeplex
		/// </summary>
		/// <param name="nodeId">The node id.</param>
		/// <param name="propertyAlias">The property alias.</param>
		/// <returns></returns>
		public static PreValue[] GetPreValues(int nodeId, string propertyAlias)
		{
			var node = new Node(nodeId);
			var xml = node.ToXml();

			if (xml != null && xml.Attributes != null)
			{
				int nodeType;
				int.TryParse(xml.Attributes["nodeType"].Value, out nodeType);

				foreach (var property in PropertyType.GetAll().Where(x => x.ContentTypeId.Equals(nodeType) && x.Alias.Equals(propertyAlias)))
				{
					var dtd = property.DataTypeDefinition;

					return GetPreValues(dtd.Id);
				}
			}

			return new List<PreValue>().ToArray();
		} 

		/// <summary>
		/// Reorders the prevalue.
		/// </summary>
		/// <param name="preValueId">The prevalue id.</param>
		/// <param name="sortOrder">The sort order.</param>
		/// <returns></returns>
		public static bool ReorderPreValue(int preValueId, int sortOrder)
		{
            var result = ApplicationContext.Current.DatabaseContext.Database.Execute(
                "UPDATE cmsDataTypePreValues SET sortorder = @sortorder WHERE id = @prevalueid",
                new { prevalueid = preValueId, sortOrder = sortOrder });

			if (result > 0)
			{
				return true;
			}

			return false;
		}
	}
}