using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.Composing;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;
using Umbraco.Core.Security;

namespace Umbraco.Core.Models
{
    public static class UserExtensions
    {
        public static IEnumerable<string> GetPermissions(this IUser user, string path, IUserService userService)
        {
            return userService.GetPermissionsForPath(user, path).GetAllPermissions();
        }

        public static bool HasSectionAccess(this IUser user, string app)
        {
            var apps = user.AllowedSections;
            return apps.Any(uApp => uApp.InvariantEquals(app));
        }

        /// <summary>
        /// Determines whether this user is the 'super' user.
        /// </summary>
        public static bool IsSuper(this IUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            return user.Id == Constants.Security.SuperUserId;
        }

        /// <summary>
        /// Determines whether this user belongs to the administrators group.
        /// </summary>
        /// <remarks>The 'super' user does not automatically belongs to the administrators group.</remarks>
        public static bool IsAdmin(this IUser user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            return user.Groups != null && user.Groups.Any(x => x.Alias == Constants.Security.AdminGroupAlias);
        }

        /// <summary>
        /// Tries to lookup the user's Gravatar to see if the endpoint can be reached, if so it returns the valid URL
        /// </summary>
        /// <param name="user"></param>
        /// <param name="cache"></param>
        /// <returns>
        /// A list of 5 different sized avatar URLs
        /// </returns>
        internal static string[] GetUserAvatarUrls(this IUser user, IAppCache cache)
        {
            // If FIPS is required, never check the Gravatar service as it only supports MD5 hashing.  
            // Unfortunately, if the FIPS setting is enabled on Windows, using MD5 will throw an exception
            // and the website will not run.
            // Also, check if the user has explicitly removed all avatars including a Gravatar, this will be possible and the value will be "none"
            if (user.Avatar == "none" || CryptoConfig.AllowOnlyFipsAlgorithms)
            {
                return new string[0];
            }

            if (user.Avatar.IsNullOrWhiteSpace())
            {
                var gravatarHash = user.Email.GenerateHash<MD5>();
                var gravatarUrl = "https://www.gravatar.com/avatar/" + gravatarHash + "?d=404";

                //try Gravatar
                var gravatarAccess = cache.GetCacheItem<bool>("UserAvatar" + user.Id, () =>
                {
                    // Test if we can reach this URL, will fail when there's network or firewall errors
                    var request = (HttpWebRequest)WebRequest.Create(gravatarUrl);
                    // Require response within 10 seconds
                    request.Timeout = 10000;
                    try
                    {
                        using ((HttpWebResponse)request.GetResponse()) { }
                    }
                    catch (Exception)
                    {
                        // There was an HTTP or other error, return an null instead
                        return false;
                    }
                    return true;
                });

                if (gravatarAccess)
                {
                    return new[]
                    {
                        gravatarUrl  + "&s=30",
                        gravatarUrl  + "&s=60",
                        gravatarUrl  + "&s=90",
                        gravatarUrl  + "&s=150",
                        gravatarUrl  + "&s=300"
                    };
                }

                return new string[0];
            }

            //use the custom avatar
            var avatarUrl = Current.MediaFileSystem.GetUrl(user.Avatar);
            var urlGenerator = Current.ImageUrlGenerator;
            return new[]
            {
                urlGenerator.GetImageUrl(new ImageUrlGenerationOptions(avatarUrl) { ImageCropMode = "crop", Width = 30, Height = 30 }),
                urlGenerator.GetImageUrl(new ImageUrlGenerationOptions(avatarUrl) { ImageCropMode = "crop", Width = 60, Height = 60 }),
                urlGenerator.GetImageUrl(new ImageUrlGenerationOptions(avatarUrl) { ImageCropMode = "crop", Width = 90, Height = 90 }),
                urlGenerator.GetImageUrl(new ImageUrlGenerationOptions(avatarUrl) { ImageCropMode = "crop", Width = 150, Height = 150 }),
                urlGenerator.GetImageUrl(new ImageUrlGenerationOptions(avatarUrl) { ImageCropMode = "crop", Width = 300, Height = 300 })
            };

        }

        /// <summary>
        /// Returns the culture info associated with this user, based on the language they're assigned to in the back office
        /// </summary>
        /// <param name="user"></param>
        /// <param name="textService"></param>
        /// <param name="globalSettings"></param>
        /// <returns></returns>
        public static CultureInfo GetUserCulture(this IUser user, ILocalizedTextService textService, IGlobalSettings globalSettings)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (textService == null) throw new ArgumentNullException(nameof(textService));
            return GetUserCulture(user.Language, textService, globalSettings);
        }

        internal static CultureInfo GetUserCulture(string userLanguage, ILocalizedTextService textService, IGlobalSettings globalSettings)
        {
            try
            {
                var culture = CultureInfo.GetCultureInfo(userLanguage.Replace("_", "-"));
                // TODO: This is a hack because we store the user language as 2 chars instead of the full culture
                // which is actually stored in the language files (which are also named with 2 chars!) so we need to attempt
                // to convert to a supported full culture
                var result = textService.ConvertToSupportedCultureWithRegionCode(culture);
                return result;
            }
            catch (CultureNotFoundException)
            {
                //return the default one
                return CultureInfo.GetCultureInfo(globalSettings.DefaultUILanguage);
            }
        }

        internal static bool HasContentRootAccess(this IUser user, IEntityService entityService, AppCaches appCaches)
            => ContentPermissionsHelper.HasPathAccess(Constants.System.RootString, user.CalculateContentStartNodeIds(entityService, appCaches), Constants.System.RecycleBinContent);

        internal static bool HasContentBinAccess(this IUser user, IEntityService entityService, AppCaches appCaches)
            => ContentPermissionsHelper.HasPathAccess(Constants.System.RecycleBinContentString, user.CalculateContentStartNodeIds(entityService, appCaches), Constants.System.RecycleBinContent);

        internal static bool HasMediaRootAccess(this IUser user, IEntityService entityService, AppCaches appCaches)
            => ContentPermissionsHelper.HasPathAccess(Constants.System.RootString, user.CalculateMediaStartNodeIds(entityService, appCaches), Constants.System.RecycleBinMedia);

        internal static bool HasMediaBinAccess(this IUser user, IEntityService entityService, AppCaches appCaches)
            => ContentPermissionsHelper.HasPathAccess(Constants.System.RecycleBinMediaString, user.CalculateMediaStartNodeIds(entityService, appCaches), Constants.System.RecycleBinMedia);

        internal static bool HasPathAccess(this IUser user, IContent content, IEntityService entityService, AppCaches appCaches)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            return ContentPermissionsHelper.HasPathAccess(content.Path, user.CalculateContentStartNodeIds(entityService, appCaches), Constants.System.RecycleBinContent);
        }

        internal static bool HasPathAccess(this IUser user, IMedia media, IEntityService entityService, AppCaches appCaches)
        {
            if (media == null) throw new ArgumentNullException(nameof(media));
            return ContentPermissionsHelper.HasPathAccess(media.Path, user.CalculateMediaStartNodeIds(entityService, appCaches), Constants.System.RecycleBinMedia);
        }

        internal static bool HasContentPathAccess(this IUser user, IUmbracoEntity entity, IEntityService entityService, AppCaches appCaches)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            return ContentPermissionsHelper.HasPathAccess(entity.Path, user.CalculateContentStartNodeIds(entityService, appCaches), Constants.System.RecycleBinContent);
        }

        internal static bool HasMediaPathAccess(this IUser user, IUmbracoEntity entity, IEntityService entityService, AppCaches appCaches)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));
            return ContentPermissionsHelper.HasPathAccess(entity.Path, user.CalculateMediaStartNodeIds(entityService, appCaches), Constants.System.RecycleBinMedia);
        }

        /// <summary>
        /// Determines whether this user has access to view sensitive data
        /// </summary>
        /// <param name="user"></param>
        public static bool HasAccessToSensitiveData(this IUser user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return user.Groups != null && user.Groups.Any(x => x.Alias == Constants.Security.SensitiveDataGroupAlias);
        }

        [Obsolete("Use the overload specifying all parameters instead")]
        public static int[] CalculateContentStartNodeIds(this IUser user, IEntityService entityService)
            => CalculateContentStartNodeIds(user, entityService, Current.AppCaches);

        /// <summary>
        /// Calculate start nodes, combining groups' and user's, and excluding what's in the bin
        /// </summary>
        /// <param name="user"></param>
        /// <param name="entityService"></param>
        /// <param name="runtimeCache"></param>
        /// <returns></returns>
        public static int[] CalculateContentStartNodeIds(this IUser user, IEntityService entityService, AppCaches appCaches)
        {
            var cacheKey = CacheKeys.UserAllContentStartNodesPrefix + user.Id;
            var runtimeCache = appCaches.IsolatedCaches.GetOrCreate<IUser>();
            var result = runtimeCache.GetCacheItem(cacheKey, () =>
            {
                var gsn = user.Groups.Where(x => x.StartContentId.HasValue).Select(x => x.StartContentId.Value).Distinct().ToArray();
                var usn = user.StartContentIds;
                var vals = CombineStartNodes(UmbracoObjectTypes.Document, gsn, usn, entityService);
                return vals;
            }, TimeSpan.FromMinutes(2), true);

            return result;
        }

        [Obsolete("Use the overload specifying all parameters instead")]
        public static int[] CalculateMediaStartNodeIds(this IUser user, IEntityService entityService)
            => CalculateMediaStartNodeIds(user, entityService, Current.AppCaches);

        /// <summary>
        /// Calculate start nodes, combining groups' and user's, and excluding what's in the bin
        /// </summary>
        /// <param name="user"></param>
        /// <param name="entityService"></param>
        /// <param name="runtimeCache"></param>
        /// <returns></returns>
        public static int[] CalculateMediaStartNodeIds(this IUser user, IEntityService entityService, AppCaches appCaches)
        {
            var cacheKey = CacheKeys.UserAllMediaStartNodesPrefix + user.Id;
            var runtimeCache = appCaches.IsolatedCaches.GetOrCreate<IUser>();
            var result = runtimeCache.GetCacheItem(cacheKey, () =>
            {
                var gsn = user.Groups.Where(x => x.StartMediaId.HasValue).Select(x => x.StartMediaId.Value).Distinct().ToArray();
                var usn = user.StartMediaIds;
                var vals = CombineStartNodes(UmbracoObjectTypes.Media, gsn, usn, entityService);
                return vals;
            }, TimeSpan.FromMinutes(2), true);

            return result;
        }

        [Obsolete("Use the overload specifying all parameters instead")]
        public static string[] GetMediaStartNodePaths(this IUser user, IEntityService entityService)
            => GetMediaStartNodePaths(user, entityService, Current.AppCaches);

        public static string[] GetMediaStartNodePaths(this IUser user, IEntityService entityService, AppCaches appCaches)
        {
            var cacheKey = CacheKeys.UserMediaStartNodePathsPrefix + user.Id;
            var runtimeCache = appCaches.IsolatedCaches.GetOrCreate<IUser>();
            var result = runtimeCache.GetCacheItem(cacheKey, () =>
            {
                var startNodeIds = user.CalculateMediaStartNodeIds(entityService, appCaches);
                var vals = entityService.GetAllPaths(UmbracoObjectTypes.Media, startNodeIds).Select(x => x.Path).ToArray();
                return vals;
            }, TimeSpan.FromMinutes(2), true);

            return result;
        }

        [Obsolete("Use the overload specifying all parameters instead")]
        public static string[] GetContentStartNodePaths(this IUser user, IEntityService entityService)
            => GetContentStartNodePaths(user, entityService, Current.AppCaches);

        public static string[] GetContentStartNodePaths(this IUser user, IEntityService entityService, AppCaches appCaches)
        {
            var cacheKey = CacheKeys.UserContentStartNodePathsPrefix + user.Id;
            var runtimeCache = appCaches.IsolatedCaches.GetOrCreate<IUser>();
            var result = runtimeCache.GetCacheItem(cacheKey, () =>
            {
                var startNodeIds = user.CalculateContentStartNodeIds(entityService, appCaches);
                var vals = entityService.GetAllPaths(UmbracoObjectTypes.Document, startNodeIds).Select(x => x.Path).ToArray();
                return vals;
            }, TimeSpan.FromMinutes(2), true);

            return result;
        }

        private static bool StartsWithPath(string test, string path)
        {
            return test.StartsWith(path) && test.Length > path.Length && test[path.Length] == ',';
        }

        private static string GetBinPath(UmbracoObjectTypes objectType)
        {
            var binPath = Constants.System.Root + ",";
            switch (objectType)
            {
                case UmbracoObjectTypes.Document:
                    binPath += Constants.System.RecycleBinContent;
                    break;
                case UmbracoObjectTypes.Media:
                    binPath += Constants.System.RecycleBinMedia;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(objectType));
            }
            return binPath;
        }

        internal static int[] CombineStartNodes(UmbracoObjectTypes objectType, int[] groupSn, int[] userSn, IEntityService entityService)
        {
            // assume groupSn and userSn each don't contain duplicates

            var asn = groupSn.Concat(userSn).Distinct().ToArray();
            var paths = asn.Length > 0
                ? entityService.GetAllPaths(objectType, asn).ToDictionary(x => x.Id, x => x.Path)
                : new Dictionary<int, string>();

            paths[Constants.System.Root] = Constants.System.RootString; // entityService does not get that one

            var binPath = GetBinPath(objectType);

            var lsn = new List<int>();
            foreach (var sn in groupSn)
            {
                if (paths.TryGetValue(sn, out var snp) == false) continue; // ignore rogue node (no path)

                if (StartsWithPath(snp, binPath)) continue; // ignore bin

                if (lsn.Any(x => StartsWithPath(snp, paths[x]))) continue; // skip if something above this sn
                lsn.RemoveAll(x => StartsWithPath(paths[x], snp)); // remove anything below this sn
                lsn.Add(sn);
            }

            var usn = new List<int>();
            foreach (var sn in userSn)
            {
                if (paths.TryGetValue(sn, out var snp) == false) continue; // ignore rogue node (no path)

                if (StartsWithPath(snp, binPath)) continue; // ignore bin

                if (usn.Any(x => StartsWithPath(paths[x], snp))) continue; // skip if something below this sn
                usn.RemoveAll(x => StartsWithPath(snp, paths[x])); // remove anything above this sn
                usn.Add(sn);
            }

            foreach (var sn in usn)
            {
                var snp = paths[sn]; // has to be here now
                lsn.RemoveAll(x => StartsWithPath(snp, paths[x]) || StartsWithPath(paths[x], snp)); // remove anything above or below this sn
                lsn.Add(sn);
            }

            return lsn.ToArray();
        }
    }
}
