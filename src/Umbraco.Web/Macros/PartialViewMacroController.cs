﻿using System.Web.Mvc;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using System.Linq;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Macros
{
    /// <summary>
    /// Controller to render macro content for Partial View Macros
    /// </summary>
    [MergeParentContextViewData]
    [HideFromTypeFinder] // explicitly used: do *not* find and register it!
    internal class PartialViewMacroController : Controller
    {
        private readonly MacroModel _macro;
        private readonly IPublishedContent _content;

        public PartialViewMacroController(MacroModel macro, IPublishedContent content)
        {
            _macro = macro;
            _content = content;
        }

        /// <summary>
        /// Child action to render a macro
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
        public PartialViewResult Index()
        {
            var model = new PartialViewMacroModel(
                _content,
                _macro.Id,
                _macro.Alias,
                _macro.Name,
                _macro.Properties.ToDictionary(x => x.Key, x => (object)x.Value));
            return PartialView(_macro.MacroSource, model);
        }
    }
}
