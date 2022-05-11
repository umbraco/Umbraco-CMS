using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Mapping
{
    public class ContentVariantMapper
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedTextService _localizedTextService;
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly IContentService _contentService;
        private readonly IUserService _userService;

        public ContentVariantMapper(
            ILocalizationService localizationService,
            ILocalizedTextService localizedTextService,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            IContentService contentService,
            IUserService userService)
        {
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _localizedTextService = localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
            _contentService = contentService;
            _userService = userService;
        }
        public ContentVariantMapper(ILocalizationService localizationService, ILocalizedTextService localizedTextService)
        : this(
            localizationService,
            localizedTextService,
            StaticServiceProvider.Instance.GetRequiredService<IBackOfficeSecurityAccessor>(),
            StaticServiceProvider.Instance.GetRequiredService<IContentService>(),
            StaticServiceProvider.Instance.GetRequiredService<IUserService>())
        {
        }

        public IEnumerable<TVariant> Map<TVariant>(IContent source, MapperContext context) where TVariant : ContentVariantDisplay
        {
            var variesByCulture = source.ContentType.VariesByCulture();
            var variesBySegment = source.ContentType.VariesBySegment();

            List<TVariant> variants = new ();

            if (!variesByCulture && !variesBySegment)
            {
                // this is invariant so just map the IContent instance to ContentVariationDisplay
                var variantDisplay = context.Map<TVariant>(source);
                if (variantDisplay is not null)
                {
                    variants.Add(variantDisplay);
                }
            }
            else if (variesByCulture && !variesBySegment)
            {
                var languages = GetLanguages(context);
                variants = languages
                    .Select(language => CreateVariantDisplay<TVariant>(context, source, language, null))
                    .WhereNotNull()
                    .ToList();
            }
            else if (variesBySegment && !variesByCulture)
            {
                // Segment only
                var segments = GetSegments(source);
                variants = segments
                    .Select(segment => CreateVariantDisplay<TVariant>(context, source, null, segment))
                    .WhereNotNull()
                    .ToList();
            }
            else
            {
                // Culture and segment
                var languages = GetLanguages(context).ToList();
                var segments = GetSegments(source).ToList();

                if (languages.Count == 0 || segments.Count == 0)
                {
                    // This should not happen
                    throw new InvalidOperationException("No languages or segments available");
                }

                variants = languages
                    .SelectMany(language => segments
                        .Select(segment => CreateVariantDisplay<TVariant>(context, source, language, segment)))
                    .WhereNotNull()
                    .ToList();
            }

            return SortVariants(variants);
        }

        private IList<TVariant> SortVariants<TVariant>(IList<TVariant> variants) where TVariant : ContentVariantDisplay
        {
            if (variants.Count <= 1)
            {
                return variants;
            }

            // Default variant first, then order by language, segment.
            return variants
                .OrderBy(v => IsDefaultLanguage(v) ? 0 : 1)
                .ThenBy(v => IsDefaultSegment(v) ? 0 : 1)
                .ThenBy(v => v?.Language?.Name)
                .ThenBy(v => v.Segment)
                .ToList();
        }

        private static bool IsDefaultSegment(ContentVariantDisplay variant)
        {
            return variant.Segment == null;
        }

        private static bool IsDefaultLanguage(ContentVariantDisplay variant)
        {
            return variant.Language == null || variant.Language.IsDefault;
        }

        private IEnumerable<ContentEditing.Language> GetLanguages(MapperContext context)
        {
            var allLanguages = _localizationService.GetAllLanguages().OrderBy(x => x.Id).ToList();
            if (allLanguages.Count == 0)
            {
                // This should never happen
                return Enumerable.Empty<ContentEditing.Language>();
            }
            else
            {
                return context.MapEnumerable<ILanguage, ContentEditing.Language>(allLanguages).WhereNotNull().ToList();
            }
        }

        /// <summary>
        /// Returns all segments assigned to the content
        /// </summary>
        /// <param name="content"></param>
        /// <returns>
        /// Returns all segments assigned to the content including the default `null` segment.
        /// </returns>
        private IEnumerable<string?> GetSegments(IContent content)
        {
            // The default segment (null) is always there,
            // even when there is no property data at all yet
            var segments = new List<string?> { null };

            // Add actual segments based on the property values
            segments.AddRange(content.Properties.SelectMany(p => p.Values.Select(v => v.Segment)));

            // Do not return a segment more than once
            return segments.Distinct();
        }

        private TVariant? CreateVariantDisplay<TVariant>(MapperContext context, IContent content, ContentEditing.Language? language, string? segment) where TVariant : ContentVariantDisplay
        {
            context.SetCulture(language?.IsoCode);
            context.SetSegment(segment);

            var variantDisplay = context.Map<TVariant>(content);

            if (variantDisplay is null)
            {
                return null;
            }
            variantDisplay.Segment = segment;
            variantDisplay.Language = language;

            // Map allowed actions
            IEnumerable<IReadOnlyUserGroup>? userGroups = _backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Groups;
            bool hasAccess = false;
            if (userGroups is not null)
            {
                foreach (var group in userGroups)
                {
                    if ((variantDisplay.Language is not null && group.AllowedLanguages.Contains(variantDisplay.Language.Id)) || group.AllowedLanguages.Any() is false)
                    {
                        hasAccess = true;
                        break;
                    }
                }

                if (hasAccess)
                {
                    variantDisplay.AllowedActions = GetLanguagePermissions(content, context);
                }
            }
            variantDisplay.Name = content.GetCultureName(language?.IsoCode);
            variantDisplay.DisplayName = GetDisplayName(language, segment);

            return variantDisplay;
        }

        private string GetDisplayName(ContentEditing.Language? language, string? segment)
        {
            var isCultureVariant = language is not null;
            var isSegmentVariant = !segment.IsNullOrWhiteSpace();

            if(!isCultureVariant && !isSegmentVariant)
            {
                return _localizedTextService.Localize("general", "default");
            }

            var parts = new List<string>();

            if (isSegmentVariant)
                parts.Add(segment!);

            if (isCultureVariant)
                parts.Add(language?.Name!);

            return string.Join(" — ", parts);

        }

        // This is a bit ugly, but when languages get granular permissions this will be really useful
        // For now we just return the exact same permissions as you had on the node, if you have access via language
        private IEnumerable<string> GetLanguagePermissions(IContent content, MapperContext context)
        {
            var backOfficeSecurity = _backOfficeSecurityAccessor.BackOfficeSecurity;

            //cannot check permissions without a context
            if (backOfficeSecurity is null)
            {
                return Enumerable.Empty<string>();
            }

            IContent? parent;
            if (context.Items.TryGetValue("Parent", out var parentObj) &&
                parentObj is IContent typedParent)
            {
                parent = typedParent;
            }
            else
            {
                parent = _contentService.GetParent(content);
            }

            string path;
            if (content.HasIdentity)
                path = content.Path;
            else
            {
                path = parent == null ? "-1" : parent.Path;
            }

            // A bit of a mess, but we need to ensure that all the required values are here AND that they're the right type.
            if (context.Items.TryGetValue("CurrentUser", out var userObject) &&
                context.Items.TryGetValue("Permissions", out var permissionsObject) &&
                userObject is IUser currentUser &&
                permissionsObject is Dictionary<string, EntityPermissionSet> permissionsDict)
            {
                // If we already have permissions for a given path,
                // and the current user is the same as was used to generate the permissions, return the stored permissions.
                if (backOfficeSecurity.CurrentUser?.Id == currentUser.Id &&
                    permissionsDict.TryGetValue(path, out var permissions))
                {
                    return permissions.GetAllPermissions();
                }
            }

            // TODO: This is certainly not ideal usage here - perhaps the best way to deal with this in the future is
            // with the IUmbracoContextAccessor. In the meantime, if used outside of a web app this will throw a null
            // reference exception :(

            return _userService.GetPermissionsForPath(backOfficeSecurity.CurrentUser, path).GetAllPermissions();
        }
    }
}
