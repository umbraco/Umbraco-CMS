using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.MacroEngines
{
    public class RazorDataTypeModelStaticMappingItem
    {
        public string Raw;

        //if all of the set (non null) properties match the property data currently being evaluated
        public string PropertyTypeAlias;
        public string NodeTypeAlias;
        public Guid? DataTypeGuid;

        public string TypeName;

        public RazorDataTypeModelStaticMappingItem() { }

        public bool Applies(Guid dataTypeGuid, string nodeTypeAlias, string propertyTypeAlias)
        {
            return
            (
                (this.NodeTypeAlias != null || this.PropertyTypeAlias != null || this.DataTypeGuid != null) &&
                ((this.DataTypeGuid != null && this.DataTypeGuid == dataTypeGuid) || this.DataTypeGuid == null) &&
                ((this.PropertyTypeAlias != null && this.PropertyTypeAlias == propertyTypeAlias) || this.PropertyTypeAlias == null) &&
                ((this.NodeTypeAlias != null && this.NodeTypeAlias == nodeTypeAlias) || this.NodeTypeAlias == null)
            );
        }
    }
}
