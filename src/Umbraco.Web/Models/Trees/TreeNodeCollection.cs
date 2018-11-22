using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.Trees
{
    [CollectionDataContract(Name = "nodes", Namespace = "")]
    public sealed class TreeNodeCollection : List<TreeNode>
    {
    }
}