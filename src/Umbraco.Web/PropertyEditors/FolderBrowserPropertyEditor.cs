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
    [PropertyEditor(Constants.PropertyEditors.FolderBrowserAlias, "(Obsolete) Folder Browser", "listview", HideLabel=true, Icon="icon-folder", Group="media")]
    public class FolderBrowserPropertyEditor : ListViewPropertyEditor
    {
        protected override PreValueEditor CreatePreValueEditor()
        {
            //return an empty one
            return new PreValueEditor();
        }

        public override IDictionary<string, object> DefaultPreValues
        {
            get
            {
                var defaults = base.DefaultPreValues;

                //make the grid the default layout for media
                if (defaults.ContainsKey("layouts") && defaults["layouts"] is IEnumerable)
                {
                    defaults["layouts"] = new[]
                    {
                        new {name = "Grid", path = "views/propertyeditors/listview/layouts/grid/grid.html", icon = "icon-thumbnails-small", isSystem = 1, selected = true},
                        new {name = "List", path = "views/propertyeditors/listview/layouts/list/list.html", icon = "icon-list", isSystem = 1, selected = true}
                    };
                }

                return defaults;
            }
        }
    }
}