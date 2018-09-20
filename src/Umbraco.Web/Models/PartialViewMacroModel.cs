﻿using System.Collections.Generic;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// The model used when rendering Partial View Macros
    /// </summary>
    public class PartialViewMacroModel
    {

        public PartialViewMacroModel(IPublishedContent page,
            int macroId,
            string macroAlias,
            string macroName,
            IDictionary<string, object> macroParams)
        {
            Content = page;
            MacroParameters = macroParams;
            MacroName = macroName;
            MacroAlias = macroAlias;
            MacroId = macroId;
        }
       
        public IPublishedContent Content { get; private set; }
        public string MacroName { get; private set; }
        public string MacroAlias { get; private set; }
        public int MacroId { get; private set; }
        public IDictionary<string, object> MacroParameters { get; private set; }
    }
}
