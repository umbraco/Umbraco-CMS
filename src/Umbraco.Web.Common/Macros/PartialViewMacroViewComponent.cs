using System.Collections.Generic;
using Umbraco.Web.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Macros
{
    /// <summary>
    /// Controller to render macro content for Partial View Macros
    /// </summary>
    //[MergeParentContextViewData] // TODO is this requeired now it is a ViewComponent?
    [HideFromTypeFinder] // explicitly used: do *not* find and register it!
    internal class PartialViewMacroViewComponent : ViewComponent
    {
        private readonly MacroModel _macro;
        private readonly IPublishedContent _content;

        public PartialViewMacroViewComponent(
            MacroModel macro,
            IPublishedContent content)
        {
            _macro = macro;
            _content = content;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new PartialViewMacroModel(
                _content,
                _macro.Id,
                _macro.Alias,
                _macro.Name,
                _macro.Properties.ToDictionary(x => x.Key, x => (object)x.Value));
            var result =  View(_macro.MacroSource, model);

            return result;
        }

        private class MacroViewEngine : ICompositeViewEngine
        {
            private readonly ICompositeViewEngine _compositeViewEngine;
            private readonly IWebHostEnvironment _webHostEnvironment;

            public MacroViewEngine(ICompositeViewEngine compositeViewEngine, IWebHostEnvironment webHostEnvironment)
            {
                _compositeViewEngine = compositeViewEngine;
                _webHostEnvironment = webHostEnvironment;
            }

            public ViewEngineResult FindView(ActionContext context, string viewName, bool isMainPage)
            {
                var result = _compositeViewEngine.FindView(context, viewName, isMainPage);
                return result;
            }

            public ViewEngineResult GetView(string executingFilePath, string viewPath, bool isMainPage)
            {
                var result = _compositeViewEngine.GetView(executingFilePath, viewPath, isMainPage);
                return result;
            }

            public IReadOnlyList<IViewEngine> ViewEngines => _compositeViewEngine.ViewEngines;
        }
    }


}
