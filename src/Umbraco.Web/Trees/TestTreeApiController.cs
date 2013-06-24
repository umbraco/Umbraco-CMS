using System.Globalization;
using System.Net.Http.Formatting;
using umbraco.businesslogic;

namespace Umbraco.Web.Trees
{
    //[Tree("test", "test", "Test Tree")]
    //public class TestTreeApiController : TreeApiController
    //{
    //    protected override TreeNodeCollection GetTreeData(string id, FormDataCollection queryStrings)
    //    {
    //        var collection = new TreeNodeCollection
    //            {
    //                CreateTreeNode("1", queryStrings, "Node 1", ""),
    //                CreateTreeNode("2", queryStrings, "Node 2", ""),
    //                CreateTreeNode("3", queryStrings, "Node 3", ""),
    //                CreateTreeNode("4", queryStrings, "Node 4", ""),
    //            };
    //        collection[0].AdditionalData.Add("TestKey", "TestVal");
    //        return collection;
    //    }

    //    //protected override string RootNodeId
    //    //{
    //    //    get { return (-1).ToString(CultureInfo.InvariantCulture); }
    //    //}
    //}
}