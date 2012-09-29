using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.MacroEngines
{
	[Obsolete("use Umbraco.Core.RazorDataTypeModelStaticMappingItem instead")]
    public class RazorDataTypeModelStaticMappingItem
	{
		private readonly Umbraco.Core.Configuration.RazorDataTypeModelStaticMappingItem _realMappingItem = new Umbraco.Core.Configuration.RazorDataTypeModelStaticMappingItem();

		public string Raw
		{
			get { return _realMappingItem.Raw; }
			set { _realMappingItem.Raw = value; }
		}

        //if all of the set (non null) properties match the property data currently being evaluated
		public string PropertyTypeAlias
		{
			get { return _realMappingItem.PropertyTypeAlias; }
			set { _realMappingItem.PropertyTypeAlias = value; }
		}

		public string NodeTypeAlias
		{
			get { return _realMappingItem.NodeTypeAlias; }
			set { _realMappingItem.NodeTypeAlias = value; }
		}

		public Guid? DataTypeGuid
		{
			get { return _realMappingItem.DataTypeGuid; }
			set { _realMappingItem.DataTypeGuid = value; }
		}

		public string TypeName
		{
			get { return _realMappingItem.TypeName; }
			set { _realMappingItem.TypeName = value; }
		}

		public bool Applies(Guid dataTypeGuid, string nodeTypeAlias, string propertyTypeAlias)
		{
			return _realMappingItem.Applies(dataTypeGuid, nodeTypeAlias, propertyTypeAlias);
		}
    }
}
