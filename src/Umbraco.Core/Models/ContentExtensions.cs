using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace Umbraco.Core.Models
{
    public static class ContentExtensions
    {
        /// <summary>
        /// Set property values by alias with an annonymous object
        /// </summary>
        public static void PropertyValues(this IContent content, object value)
        {
            if (value == null)
                throw new Exception("No properties has been passed in");

            var propertyInfos = value.GetType().GetProperties();
            foreach (var propertyInfo in propertyInfos)
            {
                //Check if a PropertyType with alias exists thus being a valid property
                var propertyType = content.PropertyTypes.FirstOrDefault(x => x.Alias == propertyInfo.Name);
                if (propertyType == null)
                    throw new Exception(
                        string.Format(
                            "The property alias {0} is not valid, because no PropertyType with this alias exists",
                            propertyInfo.Name));

                //Check if a Property with the alias already exists in the collection thus being updated or inserted
                var item = content.Properties.FirstOrDefault(x => x.Alias == propertyInfo.Name);
                if (item != null)
                {
                    item.Value = propertyInfo.GetValue(value, null);
                    //Update item with newly added value
                    content.Properties.Add(item);
                }
                else
                {
                    //Create new Property to add to collection
                    var property = propertyType.CreatePropertyFromValue(propertyInfo.GetValue(value, null));
                    content.Properties.Add(property);
                }
            }
        }

        /// <summary>
        /// Creates the xml representation for the <see cref="IContent"/> object
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to generate xml for</param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        public static XElement ToXml(this IContent content)
        {
            //nodeName should match Casing.SafeAliasWithForcingCheck(content.ContentType.Alias);
            var nodeName = content.ContentType.Alias.ToUmbracoAlias(StringAliasCaseType.CamelCase, true);
            
            var xml = new XElement(nodeName,
                                   new XAttribute("id", content.Id),
                                   new XAttribute("parentID", content.Level > 1 ? content.ParentId : -1),
                                   new XAttribute("level", content.Level),
                                   new XAttribute("writerID", content.Writer.Id),
                                   new XAttribute("creatorID", content.Creator.Id),
                                   new XAttribute("nodeType", content.ContentType.Id),
                                   new XAttribute("template", content.Template),//Template name versus Id
                                   new XAttribute("sortOrder", content.SortOrder),
                                   new XAttribute("createDate", content.CreateDate),
                                   new XAttribute("updateDate", content.UpdateDate),
                                   new XAttribute("nodeName", content.Name),
                                   new XAttribute("urlName", content.UrlName),//Format Url ?
                                   new XAttribute("writerName", content.Writer.Name),
                                   new XAttribute("creatorName", content.Creator.Name),
                                   new XAttribute("path", content.Path));
            
            foreach (var property in content.Properties)
            {
                if (property == null) continue;

                xml.Add(property.ToXml());

                if (property.Alias == "umbracoUrlName" && property.Value.ToString().Trim() != string.Empty)
                    xml.SetAttributeValue("urlName", property.Value);
            }

            return xml;
        }
    }
}