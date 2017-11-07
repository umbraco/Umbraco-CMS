using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Web.UI.Pages;

namespace Umbraco.Web.UI.Umbraco.Dialogs
{
    public partial class Publish : UmbracoEnsuredPage
    {
     
        protected string PageName { get; private set; }
        protected int DocumentId { get; private set; }
        protected string DocumentPath { get; private set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            int id;
            if (!int.TryParse(Request.GetItemAsString("id"), out id))
            {
                throw new InvalidOperationException("The id value must be an integer");
            }

            var doc = Services.ContentService.GetById(id);
            if (doc == null)
            {
                throw new InvalidOperationException("No document found with id " + id);
            }

            DocumentId = doc.Id;
            PageName = Server.HtmlEncode(doc.Name);
            DocumentPath = doc.Path;

        }
    }
}