using System.ComponentModel;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.FolderBrowserAlias, "Folder Browser", "folderbrowser", HideLabel=true, Icon="icon-folder", Group="media")]
    public class FolderBrowserPropertyEditor : PropertyEditor
    {

        

    }
}