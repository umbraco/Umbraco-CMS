using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [Obsolete("This is no longer used by default, use the ListViewPropertyEditor instead")]
    [PropertyEditor(Constants.PropertyEditors.FolderBrowserAlias, "(Obsolete) Folder Browser", "folderbrowser", HideLabel=true, Icon="icon-folder", Group="media")]
    public class FolderBrowserPropertyEditor : PropertyEditor
    {

    }
}