// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;

namespace Umbraco.Extensions;

/// <summary>
/// Provides extension methods that return UDIs for Umbraco entities.
/// </summary>
public static class UdiGetterExtensions
{
    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static Udi GetUdi(this IEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return entity switch
        {
            // Concrete types
            EntityContainer container => container.GetUdi(),
            // Interfaces
            IContentBase contentBase => contentBase.GetUdi(),
            IContentTypeComposition contentTypeComposition => contentTypeComposition.GetUdi(),
            IDataType dataType => dataType.GetUdi(),
            IDictionaryItem dictionaryItem => dictionaryItem.GetUdi(),
            ILanguage language => language.GetUdi(),
            IMemberGroup memberGroup => memberGroup.GetUdi(),
            IPartialView partialView => partialView.GetUdi(),
            IRelation relation => relation.GetUdi(),
            IRelationType relationType => relationType.GetUdi(),
            IScript script => script.GetUdi(),
            IStylesheet stylesheet => stylesheet.GetUdi(),
            ITemplate template => template.GetUdi(),
            IUser user => user.GetUdi(),
            IUserGroup userGroup => userGroup.GetUdi(),
            IWebhook webhook => webhook.GetUdi(),
            _ => throw new NotSupportedException($"Entity type {entity.GetType().FullName} is not supported."),
        };
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static GuidUdi GetUdi(this EntityContainer entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

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
        else if (entity.ContainedObjectType == Constants.ObjectTypes.DocumentBlueprint)
        {
            entityType = Constants.UdiEntityType.DocumentBlueprintContainer;
        }
        else
        {
            throw new NotSupportedException($"Contained object type {entity.ContainedObjectType} is not supported.");
        }

        return new GuidUdi(entityType, entity.Key).EnsureClosed();
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static GuidUdi GetUdi(this IContentBase entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return entity switch
        {
            IContent content => content.GetUdi(),
            IMedia media => media.GetUdi(),
            IMember member => member.GetUdi(),
            _ => throw new NotSupportedException($"Content base type {entity.GetType().FullName} is not supported."),
        };
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static GuidUdi GetUdi(this IContent entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        string entityType = entity.Blueprint ? Constants.UdiEntityType.DocumentBlueprint : Constants.UdiEntityType.Document;

        return new GuidUdi(entityType, entity.Key).EnsureClosed();
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static GuidUdi GetUdi(this IMedia entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new GuidUdi(Constants.UdiEntityType.Media, entity.Key).EnsureClosed();
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static GuidUdi GetUdi(this IMember entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new GuidUdi(Constants.UdiEntityType.Member, entity.Key).EnsureClosed();
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static GuidUdi GetUdi(this IContentTypeComposition entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return entity switch
        {
            IContentType contentType => contentType.GetUdi(),
            IMediaType mediaType => mediaType.GetUdi(),
            IMemberType memberType => memberType.GetUdi(),
            _ => throw new NotSupportedException($"Composition type {entity.GetType().FullName} is not supported."),
        };
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static GuidUdi GetUdi(this IContentType entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new GuidUdi(Constants.UdiEntityType.DocumentType, entity.Key).EnsureClosed();
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static GuidUdi GetUdi(this IMediaType entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new GuidUdi(Constants.UdiEntityType.MediaType, entity.Key).EnsureClosed();
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static GuidUdi GetUdi(this IMemberType entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new GuidUdi(Constants.UdiEntityType.MemberType, entity.Key).EnsureClosed();
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static GuidUdi GetUdi(this IDataType entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new GuidUdi(Constants.UdiEntityType.DataType, entity.Key).EnsureClosed();
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static GuidUdi GetUdi(this IDictionaryItem entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new GuidUdi(Constants.UdiEntityType.DictionaryItem, entity.Key).EnsureClosed();
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static StringUdi GetUdi(this ILanguage entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new StringUdi(Constants.UdiEntityType.Language, entity.IsoCode).EnsureClosed();
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static GuidUdi GetUdi(this IMemberGroup entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new GuidUdi(Constants.UdiEntityType.MemberGroup, entity.Key).EnsureClosed();
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static StringUdi GetUdi(this IPartialView entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return GetUdiFromPath(Constants.UdiEntityType.PartialView, entity.Path);
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static GuidUdi GetUdi(this IRelation entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new GuidUdi(Constants.UdiEntityType.Relation, entity.Key).EnsureClosed();
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static GuidUdi GetUdi(this IRelationType entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new GuidUdi(Constants.UdiEntityType.RelationType, entity.Key).EnsureClosed();
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static StringUdi GetUdi(this IScript entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return GetUdiFromPath(Constants.UdiEntityType.Script, entity.Path);
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static StringUdi GetUdi(this IStylesheet entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return GetUdiFromPath(Constants.UdiEntityType.Stylesheet, entity.Path);
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static GuidUdi GetUdi(this ITemplate entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new GuidUdi(Constants.UdiEntityType.Template, entity.Key).EnsureClosed();
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static GuidUdi GetUdi(this IUser entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new GuidUdi(Constants.UdiEntityType.User, entity.Key).EnsureClosed();
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static GuidUdi GetUdi(this IUserGroup entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new GuidUdi(Constants.UdiEntityType.UserGroup, entity.Key).EnsureClosed();
    }

    /// <summary>
    /// Gets the entity identifier of the entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    public static GuidUdi GetUdi(this IWebhook entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        return new GuidUdi(Constants.UdiEntityType.Webhook, entity.Key).EnsureClosed();
    }

    /// <summary>
    /// Gets the UDI from a path.
    /// </summary>
    /// <param name="entityType">The type of the entity.</param>
    /// <param name="path">The path.</param>
    /// <returns>
    /// The entity identifier of the entity.
    /// </returns>
    private static StringUdi GetUdiFromPath(string entityType, string path)
    {
        string id = path.TrimStart(Constants.CharArrays.ForwardSlash).Replace("\\", "/");

        return new StringUdi(entityType, id).EnsureClosed();
    }
}
