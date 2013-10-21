using System;
using System.Collections.Generic;
using umbraco.NodeFactory;

namespace umbraco.editorControls.XPathCheckBoxList
{
    [Obsolete("IDataType and all other references to the legacy property editors are no longer used this will be removed from the codebase in future versions")]
    public class XPathCheckBoxList : uQuery.IGetProperty
    {
        public XPathCheckBoxList()
        {
        }

        void uQuery.IGetProperty.LoadPropertyValue(string value)
        {
            this.SelectedNodes = uQuery.GetNodesByXml(value);
        }

        public IEnumerable<Node> SelectedNodes
        {
            get;
            private set;
        }
    }
}
