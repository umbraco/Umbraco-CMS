using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.UI.Umbraco.Controls
{
    public partial class ContentTypeControlNew : global::umbraco.controls.ContentTypeControlNew
    {
        protected string DataTypeControllerUrl { get; private set; }
        protected string ContentTypeControllerUrl { get; private set; }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs"/> object that contains the event data. </param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            DataTypeControllerUrl = Url.GetUmbracoApiServiceBaseUrl<DataTypeController>(x => x.GetById(0));
            ContentTypeControllerUrl = Url.GetUmbracoApiServiceBaseUrl<ContentTypeController>(x => x.GetAssignedListViewDataType(0));
        }

        protected void dgTabs_PreRender(object sender, EventArgs e)
        {
            dgTabs.UseAccessibleHeader = true; //to make sure we render th, not td

            Table table = dgTabs.Controls[0] as Table;
            if (table != null && table.Rows.Count > 0)
            {
                // here we render <thead> and <tfoot>
                if (dgTabs.ShowHeader) 
                    table.Rows[0].TableSection = TableRowSection.TableHeader;
                if (dgTabs.ShowFooter)
                    table.Rows[table.Rows.Count - 1].TableSection = TableRowSection.TableFooter;
            }
        }

        protected void dgTabs_ItemDataBound(object sender, DataGridItemEventArgs e)
        {
            Table table = dgTabs.Controls[0] as Table;
            if (table != null && table.Rows.Count > 0)
            {
                if (dgTabs.ShowHeader)
                    table.Rows[0].TableSection = TableRowSection.TableHeader;
                if (dgTabs.ShowFooter)
                    table.Rows[table.Rows.Count - 1].TableSection = TableRowSection.TableFooter;
            }
        }
    }
}