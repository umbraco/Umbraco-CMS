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
                            new {alias = "updateDate", header = "Last edited", isSystem = 1},
                            new {alias = "updater", header = "Last edited by", isSystem = 1}
                        }
                    }
                };
            }
        }

        internal class ListViewPreValueEditor : PreValueEditor
        {

            [PreValueField("pageSize", "Page Size", "number", Description = "Number of items per page")]
            public int PageSize { get; set; }

            [PreValueField("orderBy", "Order By", "textstring", Description = "SortOrder, Name, UpdateDate, CreateDate, ContentTypeAlias, UpdateDate, Updater or Owner")]
            public int OrderBy { get; set; }

            [PreValueField("orderDirection", "Order By", "views/propertyeditors/listview/orderdirection.prevalues.html")]
            public int OrderDirection { get; set; }

            [PreValueField("includeProperties", "Include Properties", "views/propertyeditors/listview/includeproperties.prevalues.html")]
            public object IncludeProperties { get; set; }
        }


    }
}
