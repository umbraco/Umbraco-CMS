using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Web.UI.Pages;

namespace Umbraco.Web.UI.Umbraco.Dialogs
{
    public partial class Publish : UmbracoEnsuredPage
    {
        protected int TotalNodesToPublish { get; private set; }
        protected string PageName { get; private set; }
        protected int DocumentId { get; private set; }
    }
}