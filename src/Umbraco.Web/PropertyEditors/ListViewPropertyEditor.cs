using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.ListViewAlias, "List view", "listview", HideLabel = true)]
    public class ListViewPropertyEditor : PropertyEditor
    {
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new ListViewPreValueEditor();
        }

        public override IDictionary<string, object> DefaultPreValues
        {
            get
            {
                return new Dictionary<string, object>
                {
                    {"pageSize", "10"},
                    {"orderBy", "SortOrder"},
                    {"orderDirection", "asc"},
                    {
                        "includeProperties", new[]
                        {
                            new {alias = "sortOrder", header = "Sort order", isSystem = 1},
                            new {alias = "updateDate", header = "Last edited", isSystem = 1},
                            new {alias = "owner", header = "Created by", isSystem = 1}
                        }
                    }
                };
            }
        }

        internal class ListViewPreValueEditor : PreValueEditor
        {

            [PreValueField("pageSize", "Page Size", "number", Description = "Number of items per page")]
            public int PageSize { get; set; }

            [PreValueField("orderBy", "Order By", "views/propertyeditors/listview/sortby.prevalues.html",
                Description = "The default sort order for the list")]
            public int OrderBy { get; set; }

            [PreValueField("orderDirection", "Order Direction", "views/propertyeditors/listview/orderdirection.prevalues.html")]
            public int OrderDirection { get; set; }

            [PreValueField("includeProperties", "Columns Displayed", "views/propertyeditors/listview/includeproperties.prevalues.html", 
                Description = "The properties that will be displayed for each column")]
            public object IncludeProperties { get; set; }
        }


    }
}
