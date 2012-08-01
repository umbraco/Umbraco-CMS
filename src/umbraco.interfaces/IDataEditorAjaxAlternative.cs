using System;

namespace umbraco.interfaces
{
    /// <summary>
    /// The alternative Data Editor which supports AJAX
    /// </summary>
    public interface IDataEditorAjaxAlternative
    {
        /// <summary>
        /// Gets the ajax editor.
        /// </summary>
        /// <value>The ajax editor.</value>
        System.Web.UI.Control AjaxEditor { get; }
    }
}
