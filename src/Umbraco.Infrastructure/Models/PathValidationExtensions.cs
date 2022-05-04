using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Provides extension methods for path validation.
/// </summary>
internal static class PathValidationExtensions
{
    /// <summary>
    ///     Does a quick check on the entity's set path to ensure that it's valid and consistent
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static void ValidatePathWithException(this NodeDto entity)
    {
        // don't validate if it's empty and it has no id
        if (entity.NodeId == default && entity.Path.IsNullOrWhiteSpace())
        {
            return;
        }

        if (entity.Path.IsNullOrWhiteSpace())
        {
            throw new InvalidDataException(
                $"The content item {entity.NodeId} has an empty path: {entity.Path} with parentID: {entity.ParentId}");
        }

        var pathParts = entity.Path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);
        if (pathParts.Length < 2)
        {
            // a path cannot be less than 2 parts, at a minimum it must be root (-1) and it's own id
            throw new InvalidDataException(
                $"The content item {entity.NodeId} has an invalid path: {entity.Path} with parentID: {entity.ParentId}");
        }

        if (entity.ParentId != default && pathParts[^2] != entity.ParentId.ToInvariantString())
        {
            // the 2nd last id in the path must be it's parent id
            throw new InvalidDataException(
                $"The content item {entity.NodeId} has an invalid path: {entity.Path} with parentID: {entity.ParentId}");
        }
    }

    /// <summary>
    ///     Does a quick check on the entity's set path to ensure that it's valid and consistent
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static bool ValidatePath(this IUmbracoEntity entity)
    {
        // don't validate if it's empty and it has no id
        if (entity.HasIdentity == false && entity.Path.IsNullOrWhiteSpace())
        {
            return true;
        }

        if (entity.Path.IsNullOrWhiteSpace())
        {
            return false;
        }

        var pathParts = entity.Path.Split(Constants.CharArrays.Comma, StringSplitOptions.RemoveEmptyEntries);
        if (pathParts.Length < 2)
        {
            // a path cannot be less than 2 parts, at a minimum it must be root (-1) and it's own id
            return false;
        }

        if (entity.ParentId != default && pathParts[^2] != entity.ParentId.ToInvariantString())
        {
            // the 2nd last id in the path must be it's parent id
            return false;
        }

        return true;
    }

    /// <summary>
    ///     This will validate the entity's path and if it's invalid it will fix it, if fixing is required it will recursively
    ///     check and fix all ancestors if required.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="logger"></param>
    /// <param name="getParent">A callback specified to retrieve the parent entity of the entity</param>
    /// <param name="update">A callback specified to update a fixed entity</param>
    public static void EnsureValidPath<T>(
        this T entity,
        ILogger<T> logger,
        Func<T, T> getParent,
        Action<T> update)
        where T : IUmbracoEntity
    {
        if (entity.HasIdentity == false)
        {
            throw new InvalidOperationException(
                "Could not ensure the entity path, the entity has not been assigned an identity");
        }

        if (entity.ValidatePath() == false)
        {
            logger.LogWarning(
                "The content item {EntityId} has an invalid path: {EntityPath} with parentID: {EntityParentId}",
                entity.Id, entity.Path, entity.ParentId);
            if (entity.ParentId == -1)
            {
                entity.Path = string.Concat("-1,", entity.Id);

                // path changed, update it
                update(entity);
            }
            else
            {
                T? parent = getParent(entity);
                if (parent == null)
                {
                    throw new NullReferenceException("Could not ensure path for entity " + entity.Id +
                                                     " could not resolve it's parent " + entity.ParentId);
                }

                // the parent must also be valid!
                parent.EnsureValidPath(logger, getParent, update);

                entity.Path = string.Concat(parent.Path, ",", entity.Id);

                // path changed, update it
                update(entity);
            }
        }
    }
}
