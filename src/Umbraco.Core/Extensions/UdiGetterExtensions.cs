// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Extensions;

/// <summary>
///     Provides extension methods that return udis for Umbraco entities.
/// </summary>
public static class UdiGetterExtensions
{
    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static GuidUdi GetUdi(this ITemplate entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        return new GuidUdi(Constants.UdiEntityType.Template, entity.Key).EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static GuidUdi GetUdi(this IContentType entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        return new GuidUdi(Constants.UdiEntityType.DocumentType, entity.Key).EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static GuidUdi GetUdi(this IMediaType entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        return new GuidUdi(Constants.UdiEntityType.MediaType, entity.Key).EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static GuidUdi GetUdi(this IMemberType entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        return new GuidUdi(Constants.UdiEntityType.MemberType, entity.Key).EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static GuidUdi GetUdi(this IMemberGroup entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        return new GuidUdi(Constants.UdiEntityType.MemberGroup, entity.Key).EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static GuidUdi GetUdi(this IContentTypeComposition entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        string type;
        if (entity is IContentType)
        {
            type = Constants.UdiEntityType.DocumentType;
        }
        else if (entity is IMediaType)
        {
            type = Constants.UdiEntityType.MediaType;
        }
        else if (entity is IMemberType)
        {
            type = Constants.UdiEntityType.MemberType;
        }
        else
        {
            throw new NotSupportedException(string.Format(
                "Composition type {0} is not supported.",
                entity.GetType().FullName));
        }

        return new GuidUdi(type, entity.Key).EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static GuidUdi GetUdi(this IDataType entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        return new GuidUdi(Constants.UdiEntityType.DataType, entity.Key).EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static GuidUdi GetUdi(this EntityContainer entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        string entityType;
        if (entity.ContainedObjectType == Constants.ObjectTypes.DataType)
        {
            entityType = Constants.UdiEntityType.DataTypeContainer;
        }
        else if (entity.ContainedObjectType == Constants.ObjectTypes.DocumentType)
        {
            entityType = Constants.UdiEntityType.DocumentTypeContainer;
        }
        else if (entity.ContainedObjectType == Constants.ObjectTypes.MediaType)
        {
            entityType = Constants.UdiEntityType.MediaTypeContainer;
        }
        else
        {
            throw new NotSupportedException(string.Format(
                "Contained object type {0} is not supported.",
                entity.ContainedObjectType));
        }

        return new GuidUdi(entityType, entity.Key).EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static GuidUdi GetUdi(this IMedia entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        return new GuidUdi(Constants.UdiEntityType.Media, entity.Key).EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static GuidUdi GetUdi(this IContent entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        return new GuidUdi(
                entity.Blueprint ? Constants.UdiEntityType.DocumentBlueprint : Constants.UdiEntityType.Document,
                entity.Key)
            .EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static GuidUdi GetUdi(this IMember entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        return new GuidUdi(Constants.UdiEntityType.Member, entity.Key).EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static StringUdi GetUdi(this Stylesheet entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        return new StringUdi(
            Constants.UdiEntityType.Stylesheet,
            entity.Path.TrimStart(Constants.CharArrays.ForwardSlash)).EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static StringUdi GetUdi(this Script entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        return new StringUdi(Constants.UdiEntityType.Script, entity.Path.TrimStart(Constants.CharArrays.ForwardSlash))
            .EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static GuidUdi GetUdi(this IDictionaryItem entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        return new GuidUdi(Constants.UdiEntityType.DictionaryItem, entity.Key).EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static GuidUdi GetUdi(this IMacro entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        return new GuidUdi(Constants.UdiEntityType.Macro, entity.Key).EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static StringUdi GetUdi(this IPartialView entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        // we should throw on Unknown but for the time being, assume it means PartialView
        var entityType = entity.ViewType == PartialViewType.PartialViewMacro
            ? Constants.UdiEntityType.PartialViewMacro
            : Constants.UdiEntityType.PartialView;

        return new StringUdi(entityType, entity.Path.TrimStart(Constants.CharArrays.ForwardSlash)).EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static GuidUdi GetUdi(this IContentBase entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        string type;
        if (entity is IContent)
        {
            type = Constants.UdiEntityType.Document;
        }
        else if (entity is IMedia)
        {
            type = Constants.UdiEntityType.Media;
        }
        else if (entity is IMember)
        {
            type = Constants.UdiEntityType.Member;
        }
        else
        {
            throw new NotSupportedException(string.Format(
                "ContentBase type {0} is not supported.",
                entity.GetType().FullName));
        }

        return new GuidUdi(type, entity.Key).EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static GuidUdi GetUdi(this IRelationType entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        return new GuidUdi(Constants.UdiEntityType.RelationType, entity.Key).EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static StringUdi GetUdi(this ILanguage entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        return new StringUdi(Constants.UdiEntityType.Language, entity.IsoCode).EnsureClosed();
    }

    /// <summary>
    ///     Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>The entity identifier of the entity.</returns>
    public static Udi GetUdi(this IEntity entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException("entity");
        }

        // entity could eg be anything implementing IThing
        // so we have to go through casts here
        if (entity is ITemplate template)
        {
            return template.GetUdi();
        }

        if (entity is IContentType contentType)
        {
            return contentType.GetUdi();
        }

        if (entity is IMediaType mediaType)
        {
            return mediaType.GetUdi();
        }

        if (entity is IMemberType memberType)
        {
            return memberType.GetUdi();
        }

        if (entity is IMemberGroup memberGroup)
        {
            return memberGroup.GetUdi();
        }

        if (entity is IContentTypeComposition contentTypeComposition)
        {
            return contentTypeComposition.GetUdi();
        }

        if (entity is IDataType dataTypeComposition)
        {
            return dataTypeComposition.GetUdi();
        }

        if (entity is EntityContainer container)
        {
            return container.GetUdi();
        }

        if (entity is IMedia media)
        {
            return media.GetUdi();
        }

        if (entity is IContent content)
        {
            return content.GetUdi();
        }

        if (entity is IMember member)
        {
            return member.GetUdi();
        }

        if (entity is Stylesheet stylesheet)
        {
            return stylesheet.GetUdi();
        }

        if (entity is Script script)
        {
            return script.GetUdi();
        }

        if (entity is IDictionaryItem dictionaryItem)
        {
            return dictionaryItem.GetUdi();
        }

        if (entity is IMacro macro)
        {
            return macro.GetUdi();
        }

        if (entity is IPartialView partialView)
        {
            return partialView.GetUdi();
        }

        if (entity is IContentBase contentBase)
        {
            return contentBase.GetUdi();
        }

        if (entity is IRelationType relationType)
        {
            return relationType.GetUdi();
        }

        if (entity is ILanguage language)
        {
            return language.GetUdi();
        }

        throw new NotSupportedException(string.Format("Entity type {0} is not supported.", entity.GetType().FullName));
    }
}
