using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace umbraco.MacroEngines
{
    class ExamineSearchUtill
    {
        internal static DynamicNodeList ConvertSearchResultToDynamicNode(Examine.ISearchResults results)
        {
            var list = new DynamicNodeList();
            var xd = new XmlDocument();

            foreach (var result in results.OrderByDescending(x => x.Score))
            {
                var item = new DynamicBackingItem(result.Id);
            	if (item.Id == 0) continue;
            	var node = (NodeFactory.Node)item.content;
            	var examineResultXml = Umbraco.Core.XmlHelper.AddTextNode(xd, "examineScore", result.Score.ToString());
            	node.Properties.Add(new NodeFactory.Property(examineResultXml));

            	list.Add(new DynamicNode(item));
            }
            return list;
        }

    }
}
