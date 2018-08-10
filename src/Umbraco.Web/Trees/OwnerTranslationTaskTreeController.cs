using System.Net.Http.Formatting;
using umbraco;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi.Filters;
using Constants = Umbraco.Core.Constants;

namespace Umbraco.Web.Trees
{
    [UmbracoApplicationAuthorize(Constants.Applications.Translation)]
    [Tree(Constants.Applications.Translation, Constants.Trees.OwnerTranslationTasks, null, sortOrder: 2)]
    [LegacyBaseTree(typeof(loadYourTasks))]
    [PluginController("UmbracoTrees")]
    [CoreTree]
    public class OwnerTranslationTaskTreeController : TranslationTreeTreeControllerBase
    {
        public OwnerTranslationTaskTreeController() : base(TranslationTaskUserType.Owner)
        {
        }
        
        protected override TreeNode CreateRootNode(FormDataCollection queryStrings)
        {
            
            return base.CreateRootNode(queryStrings);
        }
    }
}
