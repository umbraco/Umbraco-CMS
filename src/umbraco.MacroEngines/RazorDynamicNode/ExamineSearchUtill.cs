using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace umbraco.MacroEngines
{
    class ExamineSearchUtill
    {

        internal static DynamicNodeList convertSearchResultToDynamicNode(Examine.ISearchResults results)
        {
            DynamicNodeList list = new DynamicNodeList();
            XmlDocument xd = new XmlDocument();

            foreach (var result in results.OrderByDescending(x => x.Score))
            {
                var item = new DynamicBackingItem(result.Id);
                if (item != null && item.Id != 0)
                {
                    var node = (NodeFactory.Node)item.content;
                    XmlNode examineResultXml = xmlHelper.addTextNode(xd, "examineScore", result.Score.ToString());
                    node.Properties.Add(new NodeFactory.Property(examineResultXml));

                    list.Add(new DynamicNode(item));
                }
            }
            return list;
        }

    }
}
