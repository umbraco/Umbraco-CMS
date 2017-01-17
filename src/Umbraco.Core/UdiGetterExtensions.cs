using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core
{
    /// <summary>
    /// Provides extension methods that return udis for Umbraco entities.
    /// </summary>
    public static class UdiGetterExtensions
    {
        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this ITemplate entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.DeployEntityType.Template, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IContentType entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.DeployEntityType.DocumentType, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IMediaType entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.DeployEntityType.MediaType, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IMemberType entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.DeployEntityType.MemberType, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IMemberGroup entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.DeployEntityType.MemberGroup, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IContentTypeComposition entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            string type;
            if (entity is IContentType) type = Constants.DeployEntityType.DocumentType;
            else if (entity is IMediaType) type = Constants.DeployEntityType.MediaType;
            else if (entity is IMemberType) type = Constants.DeployEntityType.MemberType;
            else throw new NotSupportedException(string.Format("Composition type {0} is not supported.", entity.GetType().FullName));
            return new GuidUdi(type, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IDataTypeDefinition entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.DeployEntityType.DataType, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this Umbraco.Core.Models.EntityContainer entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            string entityType;
            if (entity.ContainedObjectType == Constants.ObjectTypes.DataTypeGuid)
                entityType = Constants.DeployEntityType.DataTypeContainer;
            else if (entity.ContainedObjectType == Constants.ObjectTypes.DocumentTypeGuid)
                entityType = Constants.DeployEntityType.DocumentTypeContainer;
            else if (entity.ContainedObjectType == Constants.ObjectTypes.MediaTypeGuid)
                entityType = Constants.DeployEntityType.MediaTypeContainer;
            else
                throw new NotSupportedException(string.Format("Contained object type {0} is not supported.", entity.ContainedObjectType));
            return new GuidUdi(entityType, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IMedia entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.DeployEntityType.Media, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IContent entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.DeployEntityType.Document, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IMember entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.DeployEntityType.Member, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static StringUdi GetUdi(this Stylesheet entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new StringUdi(Constants.DeployEntityType.Stylesheet, entity.Path.TrimStart('/')).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static StringUdi GetUdi(this Script entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new StringUdi(Constants.DeployEntityType.Script, entity.Path.TrimStart('/')).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IDictionaryItem entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.DeployEntityType.DictionaryItem, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IMacro entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.DeployEntityType.Macro, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static StringUdi GetUdi(this IPartialView entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new StringUdi(Constants.DeployEntityType.PartialView, entity.Path.TrimStart('/')).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static StringUdi GetUdi(this IXsltFile entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new StringUdi(Constants.DeployEntityType.Xslt, entity.Path.TrimStart('/')).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IContentBase entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            string type;
            if (entity is IContent) type = Constants.DeployEntityType.Document;
            else if (entity is IMedia) type = Constants.DeployEntityType.Media;
            else if (entity is IMember) type = Constants.DeployEntityType.Member;
            else throw new NotSupportedException(string.Format("ContentBase type {0} is not supported.", entity.GetType().FullName));
            return new GuidUdi(type, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IRelationType entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.DeployEntityType.RelationType, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static Udi GetUdi(this IEntity entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            // entity could eg be anything implementing IThing
            // so we have to go through casts here

            var template = entity as ITemplate;
            if (template != null) return template.GetUdi();

            var contentType = entity as IContentType;
            if (contentType != null) return contentType.GetUdi();

            var mediaType = entity as IMediaType;
            if (mediaType != null) return mediaType.GetUdi();

            var memberType = entity as IMemberType;
            if (memberType != null) return memberType.GetUdi();

            var memberGroup = entity as IMemberGroup;
            if (memberGroup != null) return memberGroup.GetUdi();

            var contentTypeComposition = entity as IContentTypeComposition;
            if (contentTypeComposition != null) return contentTypeComposition.GetUdi();

            var dataTypeComposition = entity as IDataTypeDefinition;
            if (dataTypeComposition != null) return dataTypeComposition.GetUdi();

            var container = entity as Umbraco.Core.Models.EntityContainer;
            if (container != null) return container.GetUdi();

            var media = entity as IMedia;
            if (media != null) return media.GetUdi();

            var content = entity as IContent;
            if (content != null) return content.GetUdi();

            var member = entity as IMember;
            if (member != null) return member.GetUdi();

            var contentBase = entity as IContentBase;
            if (contentBase != null) return contentBase.GetUdi();

            var macro = entity as IMacro;
            if (macro != null) return macro.GetUdi();

            var relationType = entity as IRelationType;
            if (relationType != null) return relationType.GetUdi();

            throw new NotSupportedException(string.Format("Entity type {0} is not supported.", entity.GetType().FullName));
        }
    }
}