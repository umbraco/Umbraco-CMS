using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Trees;
using Umbraco.Cms.Web.Common.ModelBinders;

namespace Umbraco.Cms.Web.BackOffice.Trees;

/// <summary>
///     Represents an TreeNodeController
/// </summary>
public interface ITreeNodeController
{
    /// <summary>
    ///     Gets an individual tree node
    /// </summary>
    /// <param name="id"></param>
    /// <param name="queryStrings"></param>
    /// <returns></returns>
    ActionResult<TreeNode?> GetTreeNode(
        string id,
        [ModelBinder(typeof(HttpQueryStringModelBinder))]
        FormCollection? queryStrings
    );
}
