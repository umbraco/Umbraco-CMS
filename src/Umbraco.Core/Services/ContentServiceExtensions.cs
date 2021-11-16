using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Membership;

namespace Umbraco.Core.Services
{
    /// <summary>
    /// Content service extension methods
    /// </summary>
    public static class ContentServiceExtensions
    {
        #region RTE Anchor values

        private static readonly Regex AnchorRegex = new Regex("<a id=\"(.*?)\">", RegexOptions.Compiled);

        internal static IEnumerable<string> GetAnchorValuesFromRTEs(this IContentService contentService, int id, string culture = "*")
        {
            var result = new List<string>();
            var content = contentService.GetById(id);

            foreach (var contentProperty in content.Properties)
            {
                if (contentProperty.PropertyType.PropertyEditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.TinyMce))
                {
                    var value = contentProperty.GetValue(culture)?.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        result.AddRange(contentService.GetAnchorValuesFromRTEContent(value));
                    }
                }
            }
            return result;
        }


        internal static IEnumerable<string> GetAnchorValuesFromRTEContent(this IContentService contentService, string rteContent)
        {
            var result = new List<string>();
            var matches = AnchorRegex.Matches(rteContent);
            foreach (Match match in matches)
            {
                result.Add(match.Value.Split(Constants.CharArrays.DoubleQuote)[1]);
            }
            return result;
        }
        #endregion

        public static IEnumerable<IContent> GetByIds(this IContentService contentService, IEnumerable<Udi> ids)
        {
            var guids = new List<GuidUdi>();
            foreach (var udi in ids)
            {
                var guidUdi = udi as GuidUdi;
                if (guidUdi == null)
                    throw new InvalidOperationException("The UDI provided isn't of type " + typeof(GuidUdi) + " which is required by content");
                guids.Add(guidUdi);
            }

            return contentService.GetByIds(guids.Select(x => x.Guid));
        }

        /// <summary>
        /// Method to create an IContent object based on the Udi of a parent
        /// </summary>
        /// <param name="contentService"></param>
        /// <param name="name"></param>
        /// <param name="parentId"></param>
        /// <param name="contentTypeAlias"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public static IContent CreateContent(this IContentService contentService, string name, Udi parentId, string contentTypeAlias, int userId = Constants.Security.SuperUserId)
        {
            var guidUdi = parentId as GuidUdi;
            if (guidUdi == null)
                throw new InvalidOperationException("The UDI provided isn't of type " + typeof(GuidUdi) + " which is required by content");
            var parent = contentService.GetById(guidUdi.Guid);
            return contentService.Create(name, parent, contentTypeAlias, userId);
        }

        /// <summary>
        /// Remove all permissions for this user for all nodes
        /// </summary>
        /// <param name="contentService"></param>
        /// <param name="contentId"></param>
        public static void RemoveContentPermissions(this IContentService contentService, int contentId)
        {
            contentService.SetPermissions(new EntityPermissionSet(contentId, new EntityPermissionCollection()));
        }
    }
}
