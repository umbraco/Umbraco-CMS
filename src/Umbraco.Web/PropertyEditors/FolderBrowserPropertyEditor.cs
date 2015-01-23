using System.ComponentModel;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.FolderBrowserAlias, "Folder Browser", "folderbrowser", HideLabel=true)]
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