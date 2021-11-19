using System;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;

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
            return new GuidUdi(Constants.UdiEntityType.Template, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IContentType entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.UdiEntityType.DocumentType, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IMediaType entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.UdiEntityType.MediaType, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IMemberType entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.UdiEntityType.MemberType, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IMemberGroup entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.UdiEntityType.MemberGroup, entity.Key).EnsureClosed();
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
            if (entity is IContentType) type = Constants.UdiEntityType.DocumentType;
            else if (entity is IMediaType) type = Constants.UdiEntityType.MediaType;
            else if (entity is IMemberType) type = Constants.UdiEntityType.MemberType;
            else throw new NotSupportedException(string.Format("Composition type {0} is not supported.", entity.GetType().FullName));
            return new GuidUdi(type, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IDataType entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.UdiEntityType.DataType, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this EntityContainer entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            string entityType;
            if (entity.ContainedObjectType == Constants.ObjectTypes.DataType)
                entityType = Constants.UdiEntityType.DataTypeContainer;
            else if (entity.ContainedObjectType == Constants.ObjectTypes.DocumentType)
                entityType = Constants.UdiEntityType.DocumentTypeContainer;
            else if (entity.ContainedObjectType == Constants.ObjectTypes.MediaType)
                entityType = Constants.UdiEntityType.MediaTypeContainer;
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
            return new GuidUdi(Constants.UdiEntityType.Media, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IContent entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(entity.Blueprint ? Constants.UdiEntityType.DocumentBlueprint : Constants.UdiEntityType.Document, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IMember entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.UdiEntityType.Member, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static StringUdi GetUdi(this Stylesheet entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new StringUdi(Constants.UdiEntityType.Stylesheet, entity.Path.TrimStart(Constants.CharArrays.ForwardSlash)).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static StringUdi GetUdi(this Script entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new StringUdi(Constants.UdiEntityType.Script, entity.Path.TrimStart(Constants.CharArrays.ForwardSlash)).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IDictionaryItem entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.UdiEntityType.DictionaryItem, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static GuidUdi GetUdi(this IMacro entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new GuidUdi(Constants.UdiEntityType.Macro, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static StringUdi GetUdi(this IPartialView entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            // we should throw on Unknown but for the time being, assume it means PartialView
            var entityType = entity.ViewType == PartialViewType.PartialViewMacro
                ? Constants.UdiEntityType.PartialViewMacro
                : Constants.UdiEntityType.PartialView;

            return new StringUdi(entityType, entity.Path.TrimStart(Constants.CharArrays.ForwardSlash)).EnsureClosed();
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
            if (entity is IContent) type = Constants.UdiEntityType.Document;
            else if (entity is IMedia) type = Constants.UdiEntityType.Media;
            else if (entity is IMember) type = Constants.UdiEntityType.Member;
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
            return new GuidUdi(Constants.UdiEntityType.RelationType, entity.Key).EnsureClosed();
        }

        /// <summary>
        /// Gets the entity identifier of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns>The entity identifier of the entity.</returns>
        public static StringUdi GetUdi(this ILanguage entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            return new StringUdi(Constants.UdiEntityType.Language, entity.IsoCode).EnsureClosed();
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

            var dataTypeComposition = entity as IDataType;
            if (dataTypeComposition != null) return dataTypeComposition.GetUdi();

            var container = entity as EntityContainer;
            if (container != null) return container.GetUdi();

            var media = entity as IMedia;
            if (media != null) return media.GetUdi();

            var content = entity as IContent;
            if (content != null) return content.GetUdi();

            var member = entity as IMember;
            if (member != null) return member.GetUdi();

            var stylesheet = entity as Stylesheet;
            if (stylesheet != null) return stylesheet.GetUdi();

            var script = entity as Script;
            if (script != null) return script.GetUdi();

            var dictionaryItem = entity as IDictionaryItem;
            if (dictionaryItem != null) return dictionaryItem.GetUdi();

            var macro = entity as IMacro;
            if (macro != null) return macro.GetUdi();

            var partialView = entity as IPartialView;
            if (partialView != null) return partialView.GetUdi();

            var contentBase = entity as IContentBase;
            if (contentBase != null) return contentBase.GetUdi();

            var relationType = entity as IRelationType;
            if (relationType != null) return relationType.GetUdi();

            var language = entity as ILanguage;
            if (language != null) return language.GetUdi();

            throw new NotSupportedException(string.Format("Entity type {0} is not supported.", entity.GetType().FullName));
        }
    }
}
