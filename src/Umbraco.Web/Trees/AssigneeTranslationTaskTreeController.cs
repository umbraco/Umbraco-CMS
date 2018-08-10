using umbraco;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;

using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoApplicationAuthorize(Constants.Applications.Translation)]
    [LegacyBaseTree(typeof(loadOpenTasks))]
    [Tree(Constants.Applications.Translation, Constants.Trees.AssignedTranslationTasks)]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class AssigneeTranslationTaskTreeController : TranslationTreeTreeControllerBase
    {
        public AssigneeTranslationTaskTreeController() : base(TranslationTaskUserType.Assignee)
        {
        }
    }
}
