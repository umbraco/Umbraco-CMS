using System.Collections.Generic;
using System.Web.Mvc;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using umbraco.cms.businesslogic.macro;
using umbraco.interfaces;
using System.Linq;

namespace Umbraco.Web.Macros
{
    /// <summary>
    /// Controller to render macro content for Parital View Macros
    /// </summary>
    [MergeParentContextViewData]
    internal class PartialViewMacroController : Controller
    {
        private readonly UmbracoContext _umbracoContext;
        private readonly MacroModel _macro;
        private readonly INode _currentPage;

        public PartialViewMacroController(UmbracoContext umbracoContext, MacroModel macro, INode currentPage)
        {
            _umbracoContext = umbracoContext;
            _macro = macro;
            _currentPage = currentPage;
        }

        /// <summary>
        /// Child action to render a macro
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
        public PartialViewResult Index()
        {
            var model = new PartialViewMacroModel(
                _currentPage.ConvertFromNode(),
                _macro.Id,
                _macro.Alias,
                _macro.Name,
                _macro.Properties.ToDictionary(x => x.Key, x => (object)x.Value));
            return PartialView(_macro.ScriptName, model);
        }

    }
}