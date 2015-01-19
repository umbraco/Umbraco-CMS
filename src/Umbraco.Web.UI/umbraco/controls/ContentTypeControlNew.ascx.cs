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
      
    }
}