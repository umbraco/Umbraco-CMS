using System;
using System.Collections.Generic;
using System.ComponentModel;
using Umbraco.Core.Models;

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

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use the constructor accepting the macro id instead")]
        public PartialViewMacroModel(IPublishedContent page, IDictionary<string, object> macroParams)
        {
            Content = page;
            MacroParameters = macroParams;
        }

	    [EditorBrowsable(EditorBrowsableState.Never)]
	    [Obsolete("Use the Content property instead")]
	    public IPublishedContent CurrentPage
	    {
	        get { return Content; }
	    }

        public IPublishedContent Content { get; private set; }
        public string MacroName { get; private set; }
        public string MacroAlias { get; private set; }
        public int MacroId { get; private set; }
        public IDictionary<string, object> MacroParameters { get; private set; }

    }
}