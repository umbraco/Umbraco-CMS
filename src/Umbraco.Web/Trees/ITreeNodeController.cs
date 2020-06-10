using System.Net.Http.Formatting;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.WebApi.Filters;
using System.Web.Http.ModelBinding;

//Migrated to .NET CORE
namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Represents an TreeNodeController
    /// </summary>
    public interface ITreeNodeController
    {
        /// <summary>
        /// Gets an individual tree node
        /// </summary>
        /// <param name="id"></param>
        /// <param name="queryStrings"></param>
        /// <returns></returns>
        TreeNode GetTreeNode(string id, [ModelBinder(typeof(HttpQueryStringModelBinder))]
            FormDataCollection queryStrings);
    }
}
