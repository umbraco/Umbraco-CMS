using System.Collections.Generic;
using umbraco.NodeFactory;

namespace umbraco.editorControls.XPathCheckBoxList
{
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
