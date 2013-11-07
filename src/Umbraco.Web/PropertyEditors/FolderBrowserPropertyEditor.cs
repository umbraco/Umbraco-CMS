using System.ComponentModel;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.FolderBrowserAlias, "Folder Browser", "folderbrowser", HideLabel=true)]
    public class FolderBrowserPropertyEditor : PropertyEditor
    {

        

    }
}