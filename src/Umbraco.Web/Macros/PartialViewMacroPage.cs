using System.Web.Mvc;
using Umbraco.Core.Models;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;

namespace Umbraco.Web.Macros
{
    /// <summary>
    /// The base view class that PartialViewMacro views need to inherit from
    /// </summary>
    public abstract class PartialViewMacroPage : UmbracoViewPage<PartialViewMacroModel>
    {
        protected override void InitializePage()
        {
            base.InitializePage();
            //set the model to the current node if it is not set, this is generally not the case
            if (Model != null)
            {
                CurrentPage = Model.Content.AsDynamic();
            }
        }

        /// <summary>
        /// Returns the a DynamicPublishedContent object
        /// </summary>
        public dynamic CurrentPage { get; private set; }
    }
}