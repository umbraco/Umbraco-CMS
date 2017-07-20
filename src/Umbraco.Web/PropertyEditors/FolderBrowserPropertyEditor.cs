using System;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [Obsolete("This is no longer used by default, use the ListViewPropertyEditor instead")]
    [PropertyEditor(Constants.PropertyEditors.FolderBrowserAlias, "(Obsolete) Folder Browser", "folderbrowser", HideLabel=true, Icon="icon-folder", Group="media", IsDeprecated = true)]
    public class FolderBrowserPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public FolderBrowserPropertyEditor(ILogger logger) : base(logger)
        {
        }
    }
}
