using System.Xml.Linq;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace Umbraco.Core
{
    public static class ContentExtensions
    {

        internal static bool IsMoving(this IContentBase entity)
        {
            // Check if this entity is being moved as a descendant as part of a bulk moving operations.
            // When this occurs, only Path + Level + UpdateDate are being changed. In this case we can bypass a lot of the below
            // operations which will make this whole operation go much faster. When moving we don't need to create
            // new versions, etc... because we cannot roll this operation back anyways.
            var isMoving = entity.IsPropertyDirty(nameof(entity.Path))
                           && entity.IsPropertyDirty(nameof(entity.Level))
                           && entity.IsPropertyDirty(nameof(entity.UpdateDate));

            return isMoving;
        }


        /// <summary>
        /// Removes characters that are not valid XML characters from all entity properties
        /// of type string. See: http://stackoverflow.com/a/961504/5018
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// If this is not done then the xml cache can get corrupt and it will throw YSODs upon reading it.
        /// </remarks>
        /// <param name="entity"></param>
        public static void SanitizeEntityPropertiesForXmlStorage(this IContentBase entity)
        {
            entity.Name = entity.Name.ToValidXmlString();
            foreach (var property in entity.Properties)
            {
                foreach (var propertyValue in property.Values)
                {
                    if (propertyValue.EditedValue is string editString)
                        propertyValue.EditedValue = editString.ToValidXmlString();
                    if (propertyValue.PublishedValue is string publishedString)
                        propertyValue.PublishedValue = publishedString.ToValidXmlString();
                }
            }
        }

        /// <summary>
        /// Checks if the IContentBase has children
        /// </summary>
        /// <param name="content"></param>
        /// <param name="services"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is a bit of a hack because we need to type check!
        /// </remarks>
        internal static bool HasChildren(IContentBase content, ServiceContext services)
        {
            if (content is IContent)
            {
                return services.ContentService.HasChildren(content.Id);
            }
            if (content is IMedia)
            {
                return services.MediaService.HasChildren(content.Id);
            }
            return false;
        }

        /// <summary>
        /// Creates the full xml representation for the <see cref="IContent"/> object and all of it's descendants
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to generate xml for</param>
        /// <param name="serializer"></param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        internal static XElement ToDeepXml(this IContent content, IEntityXmlSerializer serializer)
        {
            return serializer.Serialize(content, false, true);
        }

        /// <summary>
        /// Creates the xml representation for the <see cref="IContent"/> object
        /// </summary>
        /// <param name="content"><see cref="IContent"/> to generate xml for</param>
        /// <param name="serializer"></param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        public static XElement ToXml(this IContent content, IEntityXmlSerializer serializer)
        {
            return serializer.Serialize(content, false, false);
        }


        /// <summary>
        /// Creates the xml representation for the <see cref="IMedia"/> object
        /// </summary>
        /// <param name="media"><see cref="IContent"/> to generate xml for</param>
        /// <param name="serializer"></param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        public static XElement ToXml(this IMedia media, IEntityXmlSerializer serializer)
        {
            return serializer.Serialize(media);
        }

        /// <summary>
        /// Creates the xml representation for the <see cref="IMember"/> object
        /// </summary>
        /// <param name="member"><see cref="IMember"/> to generate xml for</param>
        /// <param name="serializer"></param>
        /// <returns>Xml representation of the passed in <see cref="IContent"/></returns>
        public static XElement ToXml(this IMember member, IEntityXmlSerializer serializer)
        {
            return serializer.Serialize(member);
        }
    }
}
