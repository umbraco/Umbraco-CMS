using System;
using System.ComponentModel;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [Obsolete("This is no longer used by default, use the ListViewPropertyEditor instead")]
    [PropertyEditor(Constants.PropertyEditors.FolderBrowserAlias, "(Obsolete) Folder Browser", "listview", HideLabel=true, Icon="icon-folder", Group="media")]
    public class FolderBrowserPropertyEditor : ListViewPropertyEditor
    {
        protected override PreValueEditor CreatePreValueEditor()
        {
            //return an empty one
            return new PreValueEditor();
        }
    }
}