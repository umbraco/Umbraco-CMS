using System.Net;
using System.Security.Cryptography;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

public static class UserExtensions
{
    /// <summary>
    ///     Tries to lookup the user's Gravatar to see if the endpoint can be reached, if so it returns the valid URL
    /// </summary>
    /// <param name="user"></param>
    /// <param name="cache"></param>
    /// <param name="mediaFileManager"></param>
    /// <param name="imageUrlGenerator"></param>
    /// <returns>
    ///     A list of 5 different sized avatar URLs
    /// </returns>
    public static string[] GetUserAvatarUrls(this IUser user, IAppCache cache, MediaFileManager mediaFileManager, IImageUrlGenerator imageUrlGenerator)
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
            var gravatarHash = user.Email?.GenerateHash<MD5>();
            var gravatarUrl = "https://www.gravatar.com/avatar/" + gravatarHash + "?d=404";

            // try Gravatar
            var gravatarAccess = cache.GetCacheItem("UserAvatar" + user.Id, () =>
            {
                // Test if we can reach this URL, will fail when there's network or firewall errors
                var request = (HttpWebRequest)WebRequest.Create(gravatarUrl);

                // Require response within 10 seconds
                request.Timeout = 10000;
                try
                {
                    using ((HttpWebResponse)request.GetResponse())
                    {
                    }
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
                    gravatarUrl + "&s=30", gravatarUrl + "&s=60", gravatarUrl + "&s=90", gravatarUrl + "&s=150",
                    gravatarUrl + "&s=300",
                };
            }

            return new string[0];
        }

        // use the custom avatar
        var avatarUrl = mediaFileManager.FileSystem.GetUrl(user.Avatar);
        return new[]
        {
            imageUrlGenerator.GetImageUrl(new ImageUrlGenerationOptions(avatarUrl)
            {
                ImageCropMode = ImageCropMode.Crop, Width = 30, Height = 30,
            }),
            imageUrlGenerator.GetImageUrl(new ImageUrlGenerationOptions(avatarUrl)
            {
                ImageCropMode = ImageCropMode.Crop, Width = 60, Height = 60,
            }),
            imageUrlGenerator.GetImageUrl(new ImageUrlGenerationOptions(avatarUrl)
            {
                ImageCropMode = ImageCropMode.Crop, Width = 90, Height = 90,
            }),
            imageUrlGenerator.GetImageUrl(new ImageUrlGenerationOptions(avatarUrl)
            {
                ImageCropMode = ImageCropMode.Crop, Width = 150, Height = 150,
            }),
            imageUrlGenerator.GetImageUrl(new ImageUrlGenerationOptions(avatarUrl)
            {
                ImageCropMode = ImageCropMode.Crop, Width = 300, Height = 300,
            }),
        }.WhereNotNull().ToArray();
    }

    public static bool HasPathAccess(this IUser user, IContent content, IEntityService entityService, AppCaches appCaches)
    {
        if (content == null)
        {
            throw new ArgumentNullException(nameof(content));
        }

        return ContentPermissions.HasPathAccess(
            content.Path,
            user.CalculateContentStartNodeIds(entityService, appCaches),
            Constants.System.RecycleBinContent);
    }

    internal static bool HasContentRootAccess(this IUser user, IEntityService entityService, AppCaches appCaches) =>
        ContentPermissions.HasPathAccess(
            Constants.System.RootString,
            user.CalculateContentStartNodeIds(entityService, appCaches),
            Constants.System.RecycleBinContent);

    internal static bool HasContentBinAccess(this IUser user, IEntityService entityService, AppCaches appCaches) =>
        ContentPermissions.HasPathAccess(
            Constants.System.RecycleBinContentString,
            user.CalculateContentStartNodeIds(entityService, appCaches),
            Constants.System.RecycleBinContent);

    internal static bool HasMediaRootAccess(this IUser user, IEntityService entityService, AppCaches appCaches) =>
        ContentPermissions.HasPathAccess(
            Constants.System.RootString,
            user.CalculateMediaStartNodeIds(entityService, appCaches),
            Constants.System.RecycleBinMedia);

    internal static bool HasMediaBinAccess(this IUser user, IEntityService entityService, AppCaches appCaches) =>
        ContentPermissions.HasPathAccess(
            Constants.System.RecycleBinMediaString,
            user.CalculateMediaStartNodeIds(entityService, appCaches),
            Constants.System.RecycleBinMedia);

    public static bool HasPathAccess(this IUser user, IMedia? media, IEntityService entityService, AppCaches appCaches)
    {
        if (media == null)
        {
            throw new ArgumentNullException(nameof(media));
        }

        return ContentPermissions.HasPathAccess(media.Path, user.CalculateMediaStartNodeIds(entityService, appCaches), Constants.System.RecycleBinMedia);
    }

    public static bool HasContentPathAccess(this IUser user, IUmbracoEntity entity, IEntityService entityService, AppCaches appCaches)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        return ContentPermissions.HasPathAccess(
            entity.Path,
            user.CalculateContentStartNodeIds(entityService, appCaches),
            Constants.System.RecycleBinContent);
    }

    public static bool HasMediaPathAccess(this IUser user, IUmbracoEntity entity, IEntityService entityService, AppCaches appCaches)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        return ContentPermissions.HasPathAccess(entity.Path, user.CalculateMediaStartNodeIds(entityService, appCaches), Constants.System.RecycleBinMedia);
    }

    /// <summary>
    ///     Determines whether this user has access to view sensitive data
    /// </summary>
    /// <param name="user"></param>
    public static bool HasAccessToSensitiveData(this IUser user)
    {
        if (user == null)
        {
            throw new ArgumentNullException("user");
        }

        return user.Groups != null && user.Groups.Any(x => x.Alias == Constants.Security.SensitiveDataGroupAlias);
    }

    /// <summary>
    ///     Calculate start nodes, combining groups' and user's, and excluding what's in the bin
    /// </summary>
    public static int[]? CalculateAllowedLanguageIds(this IUser user, ILocalizationService localizationService)
    {
        var hasAccessToAllLanguages = user.Groups.Any(x => x.HasAccessToAllLanguages);

        return hasAccessToAllLanguages
            ? localizationService.GetAllLanguages().Select(x => x.Id).ToArray()
            : user.Groups.SelectMany(x => x.AllowedLanguages).Distinct().ToArray();
    }

    public static int[]? CalculateContentStartNodeIds(this IUser user, IEntityService entityService, AppCaches appCaches)
    {
        var cacheKey = CacheKeys.UserAllContentStartNodesPrefix + user.Id;
        IAppPolicyCache runtimeCache = appCaches.IsolatedCaches.GetOrCreate<IUser>();
        var result = runtimeCache.GetCacheItem(
            cacheKey,
            () =>
        {
            // This returns a nullable array even though we're checking if items have value and there cannot be null
            // We use Cast<int> to recast into non-nullable array
            var gsn = user.Groups.Where(x => x.StartContentId is not null).Select(x => x.StartContentId).Distinct()
                .Cast<int>().ToArray();
            var usn = user.StartContentIds;
            if (usn is not null)
            {
                var vals = CombineStartNodes(UmbracoObjectTypes.Document, gsn, usn, entityService);
                return vals;
            }

            return null;
        },
            TimeSpan.FromMinutes(2),
            true);

        return result;
    }

    /// <summary>
    ///     Calculate start nodes, combining groups' and user's, and excluding what's in the bin
    /// </summary>
    /// <param name="user"></param>
    /// <param name="entityService"></param>
    /// <param name="appCaches"></param>
    /// <returns></returns>
    public static int[]? CalculateMediaStartNodeIds(this IUser user, IEntityService entityService, AppCaches appCaches)
    {
        var cacheKey = CacheKeys.UserAllMediaStartNodesPrefix + user.Id;
        IAppPolicyCache runtimeCache = appCaches.IsolatedCaches.GetOrCreate<IUser>();
        var result = runtimeCache.GetCacheItem(
            cacheKey,
            () =>
        {
            var gsn = user.Groups.Where(x => x.StartMediaId.HasValue).Select(x => x.StartMediaId!.Value).Distinct()
                .ToArray();
            var usn = user.StartMediaIds;
            if (usn is not null)
            {
                var vals = CombineStartNodes(UmbracoObjectTypes.Media, gsn, usn, entityService);
                return vals;
            }

            return null;
        },
            TimeSpan.FromMinutes(2),
            true);

        return result;
    }

    public static string[]? GetMediaStartNodePaths(this IUser user, IEntityService entityService, AppCaches appCaches)
    {
        var cacheKey = CacheKeys.UserMediaStartNodePathsPrefix + user.Id;
        IAppPolicyCache runtimeCache = appCaches.IsolatedCaches.GetOrCreate<IUser>();
        var result = runtimeCache.GetCacheItem(
            cacheKey,
            () =>
        {
            var startNodeIds = user.CalculateMediaStartNodeIds(entityService, appCaches);
            var vals = entityService.GetAllPaths(UmbracoObjectTypes.Media, startNodeIds).Select(x => x.Path).ToArray();
            return vals;
        },
            TimeSpan.FromMinutes(2),
            true);

        return result;
    }

    public static string[]? GetContentStartNodePaths(this IUser user, IEntityService entityService, AppCaches appCaches)
    {
        var cacheKey = CacheKeys.UserContentStartNodePathsPrefix + user.Id;
        IAppPolicyCache runtimeCache = appCaches.IsolatedCaches.GetOrCreate<IUser>();
        var result = runtimeCache.GetCacheItem(
            cacheKey,
            () =>
        {
            var startNodeIds = user.CalculateContentStartNodeIds(entityService, appCaches);
            var vals = entityService.GetAllPaths(UmbracoObjectTypes.Document, startNodeIds).Select(x => x.Path)
                .ToArray();
            return vals;
        },
            TimeSpan.FromMinutes(2),
            true);

        return result;
    }

    internal static int[] CombineStartNodes(UmbracoObjectTypes objectType, int[] groupSn, int[] userSn, IEntityService entityService)
    {
        // assume groupSn and userSn each don't contain duplicates
        var asn = groupSn.Concat(userSn).Distinct().ToArray();
        Dictionary<int, string> paths = asn.Length > 0
            ? entityService.GetAllPaths(objectType, asn).ToDictionary(x => x.Id, x => x.Path)
            : new Dictionary<int, string>();

        paths[Constants.System.Root] = Constants.System.RootString; // entityService does not get that one

        var binPath = GetBinPath(objectType);

        var lsn = new List<int>();
        foreach (var sn in groupSn)
        {
            if (paths.TryGetValue(sn, out var snp) == false)
            {
                continue; // ignore rogue node (no path)
            }

            if (StartsWithPath(snp, binPath))
            {
                continue; // ignore bin
            }

            if (lsn.Any(x => StartsWithPath(snp, paths[x])))
            {
                continue; // skip if something above this sn
            }

            lsn.RemoveAll(x => StartsWithPath(paths[x], snp)); // remove anything below this sn
            lsn.Add(sn);
        }

        var usn = new List<int>();
        foreach (var sn in userSn)
        {
            if (paths.TryGetValue(sn, out var snp) == false)
            {
                continue; // ignore rogue node (no path)
            }

            if (StartsWithPath(snp, binPath))
            {
                continue; // ignore bin
            }

            if (usn.Any(x => StartsWithPath(paths[x], snp)))
            {
                continue; // skip if something below this sn
            }

            usn.RemoveAll(x => StartsWithPath(snp, paths[x])); // remove anything above this sn
            usn.Add(sn);
        }

        foreach (var sn in usn)
        {
            var snp = paths[sn]; // has to be here now
            lsn.RemoveAll(x =>
                StartsWithPath(snp, paths[x]) ||
                StartsWithPath(paths[x], snp)); // remove anything above or below this sn
            lsn.Add(sn);
        }

        return lsn.ToArray();
    }

    private static bool StartsWithPath(string test, string path) =>
        test.StartsWith(path) && test.Length > path.Length && test[path.Length] == ',';

    private static string GetBinPath(UmbracoObjectTypes objectType)
    {
        var binPath = Constants.System.RootString + ",";
        switch (objectType)
        {
            case UmbracoObjectTypes.Document:
                binPath += Constants.System.RecycleBinContentString;
                break;
            case UmbracoObjectTypes.Media:
                binPath += Constants.System.RecycleBinMediaString;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(objectType));
        }

        return binPath;
    }
}
