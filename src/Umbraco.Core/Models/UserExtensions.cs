using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using Umbraco.Core.Cache;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Models.Membership;
using Umbraco.Core.Services;

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
        /// Tries to lookup the user's gravatar to see if the endpoint can be reached, if so it returns the valid URL
        /// </summary>
        /// <param name="user"></param>
        /// <param name="staticCache"></param>
        /// <returns>
        /// A list of 5 different sized avatar URLs
        /// </returns>
        internal static string[] GetUserAvatarUrls(this IUser user, ICacheProvider staticCache)
        {
            // If FIPS is required, never check the Gravatar service as it only supports MD5 hashing.  
            // Unfortunately, if the FIPS setting is enabled on Windows, using MD5 will throw an exception
            // and the website will not run.
            // Also, check if the user has explicitly removed all avatars including a gravatar, this will be possible and the value will be "none"
            if (user.Avatar == "none" || CryptoConfig.AllowOnlyFipsAlgorithms)
            {
                return new string[0];
            }

            if (user.Avatar.IsNullOrWhiteSpace())
            {
                var gravatarHash = user.Email.ToMd5();
                var gravatarUrl = "https://www.gravatar.com/avatar/" + gravatarHash + "?d=404";

                //try gravatar
                var gravatarAccess = staticCache.GetCacheItem<bool>("UserAvatar" + user.Id, () =>
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
            var avatarUrl = FileSystemProviderManager.Current.MediaFileSystem.GetUrl(user.Avatar);
            return new[]
            {
                avatarUrl  + "?width=30&height=30&mode=crop",
                avatarUrl  + "?width=60&height=60&mode=crop",
                avatarUrl  + "?width=90&height=90&mode=crop",
                avatarUrl  + "?width=150&height=150&mode=crop",
                avatarUrl  + "?width=300&height=300&mode=crop"
            };

        }

        /// <summary>
        /// Returns the culture info associated with this user, based on the language they're assigned to in the back office
        /// </summary>
        /// <param name="user"></param>
        /// <param name="textService"></param>
        /// <returns></returns>
        public static CultureInfo GetUserCulture(this IUser user, ILocalizedTextService textService)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (textService == null) throw new ArgumentNullException("textService");
            return GetUserCulture(user.Language, textService);
        }

        internal static CultureInfo GetUserCulture(string userLanguage, ILocalizedTextService textService)
        {
            try
            {
                var culture = CultureInfo.GetCultureInfo(userLanguage.Replace("_", "-"));
                //TODO: This is a hack because we store the user language as 2 chars instead of the full culture
                // which is actually stored in the language files (which are also named with 2 chars!) so we need to attempt
                // to convert to a supported full culture
                var result = textService.ConvertToSupportedCultureWithRegionCode(culture);
                return result;
            }
            catch (CultureNotFoundException)
            {
                //return the default one
                return CultureInfo.GetCultureInfo(GlobalSettings.DefaultUILanguage);
            }
        }

        internal static bool HasContentRootAccess(this IUser user, IEntityService entityService)
        {
            return HasPathAccess(Constants.System.Root.ToInvariantString(), user.CalculateContentStartNodeIds(entityService), Constants.System.RecycleBinContent);
        }

        internal static bool HasContentBinAccess(this IUser user, IEntityService entityService)
        {
            return HasPathAccess(Constants.System.RecycleBinContent.ToInvariantString(), user.CalculateContentStartNodeIds(entityService), Constants.System.RecycleBinContent);
        }

        internal static bool HasMediaRootAccess(this IUser user, IEntityService entityService)
        {
            return HasPathAccess(Constants.System.Root.ToInvariantString(), user.CalculateMediaStartNodeIds(entityService), Constants.System.RecycleBinMedia);
        }

        internal static bool HasMediaBinAccess(this IUser user, IEntityService entityService)
        {
            return HasPathAccess(Constants.System.RecycleBinMedia.ToInvariantString(), user.CalculateMediaStartNodeIds(entityService), Constants.System.RecycleBinMedia);
        }

        internal static bool HasPathAccess(this IUser user, IContent content, IEntityService entityService)
        {
            return HasPathAccess(content.Path, user.CalculateContentStartNodeIds(entityService), Constants.System.RecycleBinContent);
        }

        internal static bool HasPathAccess(this IUser user, IMedia media, IEntityService entityService)
        {
            return HasPathAccess(media.Path, user.CalculateMediaStartNodeIds(entityService), Constants.System.RecycleBinMedia);
        }

        internal static bool HasPathAccess(this IUser user, IUmbracoEntity entity, IEntityService entityService, int recycleBinId)
        {
            switch (recycleBinId)
            {
                case Constants.System.RecycleBinMedia:
                    return HasPathAccess(entity.Path, user.CalculateMediaStartNodeIds(entityService), recycleBinId);
                case Constants.System.RecycleBinContent:
                    return HasPathAccess(entity.Path, user.CalculateContentStartNodeIds(entityService), recycleBinId);
                default:
                    throw new NotSupportedException("Path access is only determined on content or media");
            }
        }
        
        internal static bool HasPathAccess(string path, int[] startNodeIds, int recycleBinId)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Value cannot be null or whitespace.", "path");

            // check for no access
            if (startNodeIds.Length == 0)
                return false;

            // check for root access
            if (startNodeIds.Contains(Constants.System.Root))
                return true;

            var formattedPath = string.Concat(",", path, ",");

            // only users with root access have access to the recycle bin,
            // if the above check didn't pass then access is denied
            if (formattedPath.Contains(string.Concat(",", recycleBinId, ",")))
                return false;

            // check for a start node in the path
            return startNodeIds.Any(x => formattedPath.Contains(string.Concat(",", x, ",")));
        }

        internal static bool IsInBranchOfStartNode(this IUser user, IUmbracoEntity entity, IEntityService entityService, int recycleBinId, out bool hasPathAccess)
        {
            switch (recycleBinId)
            {
                case Constants.System.RecycleBinMedia:
                    return IsInBranchOfStartNode(entity.Path, user.CalculateMediaStartNodeIds(entityService), user.GetMediaStartNodePaths(entityService), out hasPathAccess);
                case Constants.System.RecycleBinContent:
                    return IsInBranchOfStartNode(entity.Path, user.CalculateContentStartNodeIds(entityService), user.GetContentStartNodePaths(entityService), out hasPathAccess);
                default:
                    throw new NotSupportedException("Path access is only determined on content or media");
            }
        }

        internal static bool IsInBranchOfStartNode(string path, int[] startNodeIds, string[] startNodePaths, out bool hasPathAccess)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Value cannot be null or whitespace.", "path");

            hasPathAccess = false;

            // check for no access
            if (startNodeIds.Length == 0)
                return false;

            // check for root access
            if (startNodeIds.Contains(Constants.System.Root))
            {
                hasPathAccess = true;
                return true;
            }                

            //is it self?
            var self = startNodePaths.Any(x => x == path);
            if (self)
            {
                hasPathAccess = true;
                return true;
            }

            //is it ancestor?
            var ancestor = startNodePaths.Any(x => x.StartsWith(path));
            if (ancestor)
            {
                hasPathAccess = false;
                return true;
            }

            //is it descendant?
            var descendant = startNodePaths.Any(x => path.StartsWith(x));
            if (descendant)
            {
                hasPathAccess = true;
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Determines whether this user is an admin.
        /// </summary>
        /// <param name="user"></param>
        /// <returns>
        /// 	<c>true</c> if this user is admin; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsAdmin(this IUser user)
        {
            if (user == null) throw new ArgumentNullException("user");
            return user.Groups != null && user.Groups.Any(x => x.Alias == Constants.Security.AdminGroupAlias);
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

        // calc. start nodes, combining groups' and user's, and excluding what's in the bin
        public static int[] CalculateContentStartNodeIds(this IUser user, IEntityService entityService)
        {
            const string cacheKey = "AllContentStartNodes";
            //try to look them up from cache so we don't recalculate
            var valuesInUserCache = FromUserCache<int[]>(user, cacheKey);
            if (valuesInUserCache != null) return valuesInUserCache;

            var gsn = user.Groups.Where(x => x.StartContentId.HasValue).Select(x => x.StartContentId.Value).Distinct().ToArray();
            var usn = user.StartContentIds;
            var vals = CombineStartNodes(UmbracoObjectTypes.Document, gsn, usn, entityService);
            ToUserCache(user, cacheKey, vals);
            return vals;
        }

        // calc. start nodes, combining groups' and user's, and excluding what's in the bin
        public static int[] CalculateMediaStartNodeIds(this IUser user, IEntityService entityService)
        {
            const string cacheKey = "AllMediaStartNodes";
            //try to look them up from cache so we don't recalculate
            var valuesInUserCache = FromUserCache<int[]>(user, cacheKey);
            if (valuesInUserCache != null) return valuesInUserCache;

            var gsn = user.Groups.Where(x => x.StartMediaId.HasValue).Select(x => x.StartMediaId.Value).Distinct().ToArray();
            var usn = user.StartMediaIds;
            var vals = CombineStartNodes(UmbracoObjectTypes.Media, gsn, usn, entityService);
            ToUserCache(user, cacheKey, vals);
            return vals;
        }

        public static string[] GetMediaStartNodePaths(this IUser user, IEntityService entityService)
        {
            const string cacheKey = "MediaStartNodePaths";
            //try to look them up from cache so we don't recalculate
            var valuesInUserCache = FromUserCache<string[]>(user, cacheKey);
            if (valuesInUserCache != null) return valuesInUserCache;

            var startNodeIds = user.CalculateMediaStartNodeIds(entityService);
            var vals = entityService.GetAllPaths(UmbracoObjectTypes.Media, startNodeIds).Select(x => x.Path).ToArray();
            ToUserCache(user, cacheKey, vals);
            return vals;
        }

        public static string[] GetContentStartNodePaths(this IUser user, IEntityService entityService)
        {
            const string cacheKey = "ContentStartNodePaths";
            //try to look them up from cache so we don't recalculate
            var valuesInUserCache = FromUserCache<string[]>(user, cacheKey);
            if (valuesInUserCache != null) return valuesInUserCache;

            var startNodeIds = user.CalculateContentStartNodeIds(entityService);
            var vals = entityService.GetAllPaths(UmbracoObjectTypes.Document, startNodeIds).Select(x => x.Path).ToArray();
            ToUserCache(user, cacheKey, vals);
            return vals;
        }

        private static T FromUserCache<T>(IUser user, string cacheKey)
            where T: class
        {
            var entityUser = user as User;
            if (entityUser == null) return null;

            lock (entityUser.AdditionalDataLock)
            {
                object allContentStartNodes;
                return entityUser.AdditionalData.TryGetValue(cacheKey, out allContentStartNodes)
                    ? allContentStartNodes as T
                    : null;
            }
        }

        private static void ToUserCache<T>(IUser user, string cacheKey, T vals)
            where T: class
        {
            var entityUser = user as User;
            if (entityUser == null) return;

            lock (entityUser.AdditionalDataLock)
            {
                entityUser.AdditionalData[cacheKey] = vals;
            }
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
                    throw new ArgumentOutOfRangeException("objectType");
            }
            return binPath;
        }

        internal static int[] CombineStartNodes(UmbracoObjectTypes objectType, int[] groupSn, int[] userSn, IEntityService entityService)
        {
            // assume groupSn and userSn each don't contain duplicates

            var asn = groupSn.Concat(userSn).Distinct().ToArray();
            var paths = entityService.GetAllPaths(objectType, asn).ToDictionary(x => x.Id, x => x.Path);

            paths[Constants.System.Root] = Constants.System.Root.ToString(); // entityService does not get that one

            var binPath = GetBinPath(objectType);

            var lsn = new List<int>();
            foreach (var sn in groupSn)
            {
                string snp;
                if (paths.TryGetValue(sn, out snp) == false) continue; // ignore rogue node (no path)

                if (StartsWithPath(snp, binPath)) continue; // ignore bin

                if (lsn.Any(x => StartsWithPath(snp, paths[x]))) continue; // skip if something above this sn
                lsn.RemoveAll(x => StartsWithPath(paths[x], snp)); // remove anything below this sn
                lsn.Add(sn);
            }

            var usn = new List<int>();
            foreach (var sn in userSn)
            {
                string snp;
                if (paths.TryGetValue(sn, out snp) == false) continue; // ignore rogue node (no path)

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
