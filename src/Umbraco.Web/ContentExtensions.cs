using System.Xml.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web
{
    public static class ContentExtensions
    {
        /// <summary>
        /// Creates the xml representation for the <see cref="IContent"/> object
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to generate xml for</param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        public static XElement ToXml(this IContent content)
        {
            //nodeName should match Casing.SafeAliasWithForcingCheck(content.ContentType.Alias);
            var nodeName = content.ContentType.Alias.ToUmbracoAlias(StringAliasCaseType.CamelCase, true);
            var niceUrl = content.Name.Replace(" ", "-").ToLower();

            if (UmbracoContext.Current != null)
            {
                var niceUrlsProvider = UmbracoContext.Current.NiceUrlProvider;
                niceUrl = niceUrlsProvider.GetNiceUrl(content.Id);
            }

            var xml = new XElement(nodeName,
                                   new XAttribute("id", content.Id),
                                   new XAttribute("parentID", content.Level > 1 ? content.ParentId : -1),
                                   new XAttribute("level", content.Level),
                                   new XAttribute("writerID", content.WriterId),
                                   new XAttribute("creatorID", content.CreatorId),
                                   new XAttribute("nodeType", content.ContentType.Id),
                                   new XAttribute("template", content.Template.Id.ToString()),
                                   new XAttribute("sortOrder", content.SortOrder),
                                   new XAttribute("createDate", content.CreateDate),
                                   new XAttribute("updateDate", content.UpdateDate),
                                   new XAttribute("nodeName", content.Name),
                                   new XAttribute("urlName", niceUrl),//Format Url ?
                                   new XAttribute("writerName", content.GetWriterProfile().Name),
                                   new XAttribute("creatorName", content.GetCreatorProfile().Name),
                                   new XAttribute("path", content.Path));

            foreach (var property in content.Properties)
            {
                if (property == null) continue;

                xml.Add(property.ToXml());

                //Check for umbracoUrlName convention
                if (property.Alias == "umbracoUrlName" && property.Value.ToString().Trim() != string.Empty)
                    xml.SetAttributeValue("urlName", property.Value);
            }

            return xml;
        }

        /// <summary>
        /// Creates the xml representation for the <see cref="IContent"/> object
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to generate xml for</param>
        /// <param name="isPreview">Boolean indicating whether the xml should be generated for preview</param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        public static XElement ToXml(this IContent content, bool isPreview)
        {
            //TODO Do a proper implementation of this
            return content.ToXml();
        } 
    }
}