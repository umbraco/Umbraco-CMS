using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.UI.Pages;
using Umbraco.Web;
using Umbraco.Web.Routing;
using Umbraco.Web.WebServices;


namespace umbraco.dialogs
{
    public partial class AssignDomain2 : UmbracoEnsuredPage
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var nodeId = GetNodeId();
            var node = Services.ContentService.GetById(nodeId);

            if (node == null)
            {
                feedback.Text = ui.Text("assignDomain", "invalidNode");
                pane_language.Visible = false;
                pane_domains.Visible = false;
                p_buttons.Visible = false;
                return;
            }

            if (UmbracoUser.GetPermissions(node.Path).Contains(ActionAssignDomain.Instance.Letter) == false)
            {
                feedback.Text = ui.Text("assignDomain", "permissionDenied");
                pane_language.Visible = false;
                pane_domains.Visible = false;
                p_buttons.Visible = false;
                return;
            }

            pane_language.Title = ui.Text("assignDomain", "setLanguage");
            pane_domains.Title = ui.Text("assignDomain", "setDomains");
            prop_language.Text = ui.Text("assignDomain", "language");

            var nodeDomains = Services.DomainService.GetAssignedDomains(nodeId, true).ToArray();
            var wildcard = nodeDomains.FirstOrDefault(d => d.IsWildcard);

            var sb = new StringBuilder();
            sb.Append("languages: [");
            var i = 0;
            foreach (var language in ApplicationContext.Current.Services.LocalizationService.GetAllLanguages())
                sb.AppendFormat("{0}{{ \"Id\": {1}, \"Code\": \"{2}\" }}", (i++ == 0 ? "" : ","), language.Id, language.IsoCode);
            sb.Append("]\r\n");

            sb.AppendFormat(",language: {0}", wildcard == null ? "undefined" : wildcard.Language.Id.ToString());

            sb.Append(",domains: [");
            i = 0;
            foreach (var domain in nodeDomains.Where(d => d.IsWildcard == false))
                sb.AppendFormat("{0}{{ \"Name\": \"{1}\", \"Lang\": \"{2}\" }}", (i++ == 0 ? "" :","), domain.DomainName, domain.Language.Id);
            sb.Append("]\r\n");

            data.Text = sb.ToString();
        }

        protected int GetNodeId()
        {
            int nodeId;
            if (int.TryParse(Request.QueryString["id"], out nodeId) == false)
                nodeId = -1;
            return nodeId;
        }

        protected string GetRestServicePath()
        {
            const string action = "ListDomains";
            var path = Url.GetUmbracoApiService<DomainsApiController>(action);
            return path.TrimEnd(action).EnsureEndsWith('/');
        }
    }
}