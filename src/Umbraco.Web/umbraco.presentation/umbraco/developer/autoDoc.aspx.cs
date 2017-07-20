using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web.UI.Pages;

namespace umbraco.developer
{
    /// <summary>
    /// Summary description for autoDoc.
    /// </summary>
    public partial class autoDoc : UmbracoEnsuredPage
    {
        public autoDoc()
        {
            CurrentApp = Constants.Applications.Developer.ToString();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var sb = new StringBuilder();
            AppendTypes(sb, Services.ContentTypeService.GetAll());
            AppendTypes(sb, Services.MediaTypeService.GetAll());
            AppendTypes(sb, Services.MemberTypeService.GetAll());
            LabelDoc.Text = sb.ToString();
        }

        private void AppendTypes(StringBuilder text, IEnumerable<IContentTypeBase> types)
        {
            foreach (var type in types)
            {
                text.Append(
                    "<div class=\"propertyType\"><p class=\"documentType\">" + type.Name + "</p><p class=\"type\">Id: " + type.Id + ", Alias: " + type.Alias + ")</p>");
                if (type.PropertyTypes.Any())
                    text.Append("<p class=\"docHeader\">Property Types:</p>");
                foreach (var pt in type.PropertyTypes)
                    text.Append(
                        "<p class=\"type\">" + pt.Id + ", " + pt.Alias + ", " + pt.Name + "</p>");
                if (type.PropertyGroups.Count > 0)
                    text.Append("<p class=\"docHeader\">Tabs:</p>");
                foreach (var t in type.PropertyGroups)
                    text.Append(
                        "<p class=\"type\">" + t.Id + ", " + t.Name + "</p>");
                if (type.AllowedContentTypes.Any())
                    text.Append("<p class=\"docHeader\">Allowed children:</p>");
                foreach (var child in type.AllowedContentTypes)
                {
                    var contentType = types.First(x => x.Id == child.Id.Value);
                    text.Append(
                        "<p class=\"type\">" + child.Id + ", " + contentType.Name + "</p>");
                }

                text.Append("</div>");
            }
        }
    }
}
