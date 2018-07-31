using System;
using System.Text;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web.UI.Pages;
using Umbraco.Web;
using Umbraco.Web.Composing;
using Umbraco.Web.Editors;
using Umbraco.Web.WebServices;
using Umbraco.Web._Legacy.Actions;


namespace umbraco.dialogs
{
    public partial class AssignDomain2 : UmbracoEnsuredPage
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            var nodeId = GetNodeId();
            CheckPathAndPermissions(nodeId, UmbracoObjectTypes.Document, ActionAssignDomain.Instance);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            var nodeId = GetNodeId();
            var node = Services.ContentService.GetById(nodeId);

            if (node == null)
            {
                feedback.Text = Services.TextService.Localize("assignDomain/invalidNode");
                pane_language.Visible = false;
                pane_domains.Visible = false;
                p_buttons.Visible = false;
                return;
            }

            pane_language.Title = Services.TextService.Localize("assignDomain/setLanguage");
            pane_domains.Title = Services.TextService.Localize("assignDomain/setDomains");
            prop_language.Text = Services.TextService.Localize("assignDomain/language");

            var nodeDomains = Services.DomainService.GetAssignedDomains(nodeId, true).ToArray();
            var wildcard = nodeDomains.FirstOrDefault(d => d.IsWildcard);

            var sb = new StringBuilder();
            sb.Append("languages: [");
            var i = 0;
            foreach (var language in Current.Services.LocalizationService.GetAllLanguages())
                sb.AppendFormat("{0}{{ \"Id\": {1}, \"Code\": \"{2}\" }}", (i++ == 0 ? "" : ","), language.Id, language.IsoCode);
            sb.Append("]\r\n");

            sb.AppendFormat(",language: {0}", wildcard == null ? "undefined" : wildcard.LanguageId.ToString());

            sb.Append(",domains: [");
            i = 0;
            foreach (var domain in nodeDomains.Where(d => d.IsWildcard == false))
                sb.AppendFormat("{0}{{ \"Name\": \"{1}\", \"Lang\": \"{2}\" }}", (i++ == 0 ? "" :","), domain.DomainName, domain.LanguageId);
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
            var path = Url.GetUmbracoApiService<ContentController>(action);
            return path.TrimEnd(action).EnsureEndsWith('/');
        }
    }
}
