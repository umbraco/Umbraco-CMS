using System;
using System.ComponentModel;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [Obsolete( "Use the media listview instead", false)]
    [PropertyEditor(Constants.PropertyEditors.FolderBrowserAlias, "Folder Browser (Obsolete)", "folderbrowser", HideLabel=true, Icon="icon-folder", Group="media")]
    public class FolderBrowserPropertyEditor : PropertyEditor
    {
        
    }
}