using System;
using System.Collections.Generic;
using System.Text;

using System.Xml;

namespace umbraco.editorControls.relatedlinks
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class RelatedLinksData : umbraco.cms.businesslogic.datatype.DefaultData
    {
        public RelatedLinksData(umbraco.cms.businesslogic.datatype.BaseDataType DataType) : base(DataType) { }

        public override System.Xml.XmlNode ToXMl(System.Xml.XmlDocument data)
        {
            if (this.Value != null && !String.IsNullOrEmpty(this.Value.ToString())) {
                XmlDocument xd = new XmlDocument();
                xd.LoadXml(this.Value.ToString());
                return data.ImportNode(xd.DocumentElement, true);
            } else {
                return base.ToXMl(data);
            }
        }

    }
}
