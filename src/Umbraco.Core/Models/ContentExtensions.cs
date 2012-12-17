using System;
using System.Linq;
using System.Xml.Linq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Services;

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
        /// Checks whether an <see cref="IContent"/> item has any published versions
        /// </summary>
        /// <param name="content"></param>
        /// <returns>True if the content has any published versiom otherwise False</returns>
        public static bool HasPublishedVersion(this IContent content)
        {
            if (content.HasIdentity == false)
                return false;

            return ServiceContext.Current.ContentService.HasPublishedVersion(content.Id);
        }

        /// <summary>
        /// Gets the <see cref="IProfile"/> for the Creator of this content.
        /// </summary>
        public static IProfile GetCreatorProfile(this IContent content)
        {
			using (var repository = RepositoryResolver.Current.Factory.CreateUserRepository(
				PetaPocoUnitOfWorkProvider.CreateUnitOfWork()))
			{
				return repository.GetProfileById(content.CreatorId);
			}
        }

        /// <summary>
        /// Gets the <see cref="IProfile"/> for the Writer of this content.
        /// </summary>
        public static IProfile GetWriterProfile(this IContent content)
        {
			using(var repository = RepositoryResolver.Current.Factory.CreateUserRepository(
				PetaPocoUnitOfWorkProvider.CreateUnitOfWork()))
			{
				return repository.GetProfileById(content.WriterId);	
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
            //var nodeName = content.ContentType.Alias.ToUmbracoAlias(StringAliasCaseType.CamelCase, true);
            var nodeName = content.ContentType.Alias;
            var niceUrl = content.Name.Replace(" ", "-").ToLower();

            var xml = new XElement(nodeName,
                                   new XAttribute("id", content.Id),
                                   new XAttribute("parentID", content.Level > 1 ? content.ParentId : -1),
                                   new XAttribute("level", content.Level),
                                   new XAttribute("writerID", content.WriterId),
                                   new XAttribute("creatorID", content.CreatorId),
                                   new XAttribute("nodeType", content.ContentType.Id),
                                   new XAttribute("template", content.Template == null ? "0" : content.Template.Id.ToString()),
                                   new XAttribute("sortOrder", content.SortOrder),
                                   new XAttribute("createDate", content.CreateDate.ToString("s")),
                                   new XAttribute("updateDate", content.UpdateDate.ToString("s")),
                                   new XAttribute("nodeName", content.Name),
                                   new XAttribute("urlName", niceUrl),//Format Url ?
                                   new XAttribute("writerName", content.GetWriterProfile().Name),
                                   new XAttribute("creatorName", content.GetCreatorProfile().Name),
                                   new XAttribute("path", content.Path),
                                   new XAttribute("isDoc", ""));

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
            //If current IContent is published we should get latest unpublished version
            return content.ToXml();
        }

        /// <summary>
        /// Creates the xml representation for the <see cref="IContent"/> object
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to generate xml for</param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        public static XElement ToXml(this IContent content)
        {
            //nodeName should match Casing.SafeAliasWithForcingCheck(content.ContentType.Alias);
            //var nodeName = content.ContentType.Alias.ToUmbracoAlias(StringAliasCaseType.CamelCase, true);
            var nodeName = content.ContentType.Alias;
            var niceUrl = content.Name.Replace(" ", "-").ToLower();

            /* NOTE Not entirely sure if this is needed, but either way the niceUrlProvider is not 
             * available from here, so it would have to be delegated
             */
            /*if (UmbracoContext.Current != null)
            {
                var niceUrlsProvider = UmbracoContext.Current.NiceUrlProvider;
                niceUrl = niceUrlsProvider.GetNiceUrl(content.Id);
            }*/

            var xml = new XElement(nodeName,
                                   new XAttribute("id", content.Id),
                                   new XAttribute("parentID", content.Level > 1 ? content.ParentId : -1),
                                   new XAttribute("level", content.Level),
                                   new XAttribute("writerID", content.WriterId),
                                   new XAttribute("creatorID", content.CreatorId),
                                   new XAttribute("nodeType", content.ContentType.Id),
                                   new XAttribute("template", content.Template == null ? "0": content.Template.Id.ToString()),
                                   new XAttribute("sortOrder", content.SortOrder),
                                   new XAttribute("createDate", content.CreateDate.ToString("s")),
                                   new XAttribute("updateDate", content.UpdateDate.ToString("s")),
                                   new XAttribute("nodeName", content.Name),
                                   new XAttribute("urlName", niceUrl),//Format Url ?
                                   new XAttribute("writerName", content.GetWriterProfile().Name),
                                   new XAttribute("creatorName", content.GetCreatorProfile().Name),
                                   new XAttribute("path", content.Path),
                                   new XAttribute("isDoc", ""));

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
            //If current IContent is published we should get latest unpublished version
            return content.ToXml();
        } 
    }
}