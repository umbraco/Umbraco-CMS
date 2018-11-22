using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Core.Models
{
    internal static class UmbracoEntityExtensions
    {
        /// <summary>
        /// Does a quick check on the entity's set path to ensure that it's valid and consistent
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static void ValidatePathWithException(this NodeDto entity)
        {
            //don't validate if it's empty and it has no id
            if (entity.NodeId == default(int) && entity.Path.IsNullOrWhiteSpace())
                return;

            if (entity.Path.IsNullOrWhiteSpace())
                throw new InvalidDataException(string.Format("The content item {0} has an empty path: {1} with parentID: {2}", entity.NodeId, entity.Path, entity.ParentId));

            var pathParts = entity.Path.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length < 2)
            {
                //a path cannot be less than 2 parts, at a minimum it must be root (-1) and it's own id
                throw new InvalidDataException(string.Format("The content item {0} has an invalid path: {1} with parentID: {2}", entity.NodeId, entity.Path, entity.ParentId));
            }

            if (entity.ParentId != default(int) && pathParts[pathParts.Length - 2] != entity.ParentId.ToInvariantString())
            {
                //the 2nd last id in the path must be it's parent id
                throw new InvalidDataException(string.Format("The content item {0} has an invalid path: {1} with parentID: {2}", entity.NodeId, entity.Path, entity.ParentId));
            }
        }

        /// <summary>
        /// Does a quick check on the entity's set path to ensure that it's valid and consistent
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool ValidatePath(this IUmbracoEntity entity)
        {
            //don't validate if it's empty and it has no id
            if (entity.HasIdentity == false && entity.Path.IsNullOrWhiteSpace())
                return true;

            if (entity.Path.IsNullOrWhiteSpace())
                return false;

            var pathParts = entity.Path.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (pathParts.Length < 2)
            {
                //a path cannot be less than 2 parts, at a minimum it must be root (-1) and it's own id
                return false;
            }

            if (entity.ParentId != default(int) && pathParts[pathParts.Length - 2] != entity.ParentId.ToInvariantString())
            {
                //the 2nd last id in the path must be it's parent id
                return false;
            }

            return true;
        }

        /// <summary>
        /// This will validate the entity's path and if it's invalid it will fix it, if fixing is required it will recursively 
        /// check and fix all ancestors if required.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="logger"></param>
        /// <param name="getParent">A callback specified to retrieve the parent entity of the entity</param>
        /// <param name="update">A callback specified to update a fixed entity</param>
        public static void EnsureValidPath<T>(this T entity, 
            ILogger logger, 
            Func<T, T> getParent,
            Action<T> update)
            where T: IUmbracoEntity
        {
            if (entity.HasIdentity == false)
                throw new InvalidOperationException("Could not ensure the entity path, the entity has not been assigned an identity");

            if (entity.ValidatePath() == false)
            {
                logger.Warn(typeof(UmbracoEntityExtensions), string.Format("The content item {0} has an invalid path: {1} with parentID: {2}", entity.Id, entity.Path, entity.ParentId));
                if (entity.ParentId == -1)
                {
                    entity.Path = string.Concat("-1,", entity.Id);
                    //path changed, update it
                    update(entity);
                }
                else
                {
                    var parent = getParent(entity);
                    if (parent == null)
                        throw new NullReferenceException("Could not ensure path for entity " + entity.Id + " could not resolve it's parent " + entity.ParentId);

                    //the parent must also be valid!
                    parent.EnsureValidPath(logger, getParent, update);

                    entity.Path = string.Concat(parent.Path, ",", entity.Id);
                    //path changed, update it
                    update(entity);
                }
            }
        }

        /// <summary>
        /// When resolved from EntityService this checks if the entity has the HasChildren flag
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool HasChildren(this IUmbracoEntity entity)
        {
            if (entity.AdditionalData.ContainsKey("HasChildren"))
            {
                var convert = entity.AdditionalData["HasChildren"].TryConvertTo<bool>();
                if (convert)
                {
                    return convert.Result;
                }
            }
            return false;
        }

    
        public static object GetAdditionalDataValueIgnoreCase(this IUmbracoEntity entity, string key, object defaultVal)
        {
            if (entity.AdditionalData.ContainsKeyIgnoreCase(key) == false) return defaultVal;
            return entity.AdditionalData.GetValueIgnoreCase(key, defaultVal);
        }

        /// <summary>
        /// When resolved from EntityService this checks if the entity has the IsContainer flag
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static bool IsContainer(this IUmbracoEntity entity)
        {
            if (entity.AdditionalData.ContainsKeyIgnoreCase("IsContainer") == false) return false;
            var val = entity.AdditionalData.GetValueIgnoreCase("IsContainer", null);
            if (val is bool && (bool) val)
            {
                return true;
            }
            return false;
        }

    }
}
