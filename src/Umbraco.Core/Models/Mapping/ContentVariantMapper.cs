using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Actions;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Mapping;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models.Mapping;

public class ContentVariantMapper
{
    private readonly ILocalizationService _localizationService;
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
    private readonly IContentService _contentService;
    private readonly IUserService _userService;
    private ContentSettings _contentSettings;

    public ContentVariantMapper(
        ILocalizationService localizationService,
        ILocalizedTextService localizedTextService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentService contentService,
        IUserService userService,
        IOptionsMonitor<ContentSettings> contentSettings)
    {
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _localizedTextService = localizedTextService ?? throw new ArgumentNullException(nameof(localizedTextService));
        _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
        _contentService = contentService;
        _userService = userService;
        _contentSettings = contentSettings.CurrentValue;
        contentSettings.OnChange(settings => _contentSettings = settings);
    }

    [Obsolete("Use constructor that takes all parameters instead")]
    public ContentVariantMapper(
        ILocalizationService localizationService,
        ILocalizedTextService localizedTextService,
        IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
        IContentService contentService,
        IUserService userService,
        IOptionsMonitor<SecuritySettings> securitySettings)
    : this(
        localizationService,
        localizedTextService,
        backOfficeSecurityAccessor,
        contentService,
        userService,
        StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<ContentSettings>>())
    {
    }

    [Obsolete("Use constructor that takes all parameters instead")]
    public ContentVariantMapper(ILocalizationService localizationService, ILocalizedTextService localizedTextService)
        : this(
            localizationService,
            localizedTextService,
            StaticServiceProvider.Instance.GetRequiredService<IBackOfficeSecurityAccessor>(),
            StaticServiceProvider.Instance.GetRequiredService<IContentService>(),
            StaticServiceProvider.Instance.GetRequiredService<IUserService>(),
            StaticServiceProvider.Instance.GetRequiredService<IOptionsMonitor<SecuritySettings>>())
    {
    }

    public IEnumerable<TVariant> Map<TVariant>(IContent source, MapperContext context)
        where TVariant : ContentVariantDisplay
    {
        var variesByCulture = source.ContentType.VariesByCulture();
        var variesBySegment = source.ContentType.VariesBySegment();

        List<TVariant> variants = new();

        if (!variesByCulture && !variesBySegment)
        {
            // this is invariant so just map the IContent instance to ContentVariationDisplay
            TVariant? variantDisplay = context.Map<TVariant>(source);
            if (variantDisplay is not null)
            {
                // Map allowed actions per language
                variantDisplay.AllowedActions = GetLanguagePermissions(source, context, variantDisplay);
                variants.Add(variantDisplay);
            }
        }
        else if (variesByCulture && !variesBySegment)
        {
            IEnumerable<ContentEditing.Language> languages = GetLanguages(context);
            variants = languages
                .Select(language => CreateVariantDisplay<TVariant>(context, source, language, null))
                .WhereNotNull()
                .ToList();
        }
        else if (variesBySegment && !variesByCulture)
        {
            // Segment only
            IEnumerable<string?> segments = GetSegments(source);
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

    private static bool IsDefaultSegment(ContentVariantDisplay variant) => variant.Segment == null;

    private IList<TVariant> SortVariants<TVariant>(IList<TVariant> variants)
        where TVariant : ContentVariantDisplay
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

    private static bool IsDefaultLanguage(ContentVariantDisplay variant) =>
        variant.Language == null || variant.Language.IsDefault;

    private IEnumerable<ContentEditing.Language> GetLanguages(MapperContext context)
    {
        var allLanguages = _localizationService.GetAllLanguages().OrderBy(x => x.Id).ToList();
        if (allLanguages.Count == 0)
        {
            // This should never happen
            return Enumerable.Empty<ContentEditing.Language>();
        }

        return context.MapEnumerable<ILanguage, ContentEditing.Language>(allLanguages).WhereNotNull().ToList();
    }

    /// <summary>
    ///     Returns all segments assigned to the content
    /// </summary>
    /// <param name="content"></param>
    /// <returns>
    ///     Returns all segments assigned to the content including the default `null` segment.
    /// </returns>
    private IEnumerable<string?> GetSegments(IContent content)
    {
        // The default segment (null) is always there,
        // even when there is no property data at all yet
        var segments = new List<string?> {null};

        // Add actual segments based on the property values
        segments.AddRange(content.Properties.SelectMany(p => p.Values.Select(v => v.Segment)));

        // Do not return a segment more than once
        return segments.Distinct();
    }

    private TVariant? CreateVariantDisplay<TVariant>(MapperContext context, IContent content, ContentEditing.Language? language, string? segment)
        where TVariant : ContentVariantDisplay
    {
        context.SetCulture(language?.IsoCode);
        context.SetSegment(segment);

        TVariant? variantDisplay = context.Map<TVariant>(content);

        if (variantDisplay is null)
        {
            return null;
        }

        variantDisplay.Segment = segment;
        variantDisplay.Language = language;

        // Map allowed actions
        variantDisplay.AllowedActions = GetLanguagePermissions(content, context, variantDisplay);
        variantDisplay.Name = content.GetCultureName(language?.IsoCode);
        variantDisplay.DisplayName = GetDisplayName(language, segment);

        return variantDisplay;
    }

    private string GetDisplayName(ContentEditing.Language? language, string? segment)
    {
        var isCultureVariant = language is not null;
        var isSegmentVariant = !segment.IsNullOrWhiteSpace();

        if (!isCultureVariant && !isSegmentVariant)
        {
            return _localizedTextService.Localize("general", "default");
        }

        var parts = new List<string>();

        if (isSegmentVariant)
        {
            parts.Add(segment!);
        }

        if (isCultureVariant)
        {
            parts.Add(language?.Name!);
        }

        return string.Join(" — ", parts);
    }

    // This is a bit ugly, but when languages get granular permissions this will be really useful
    // For now we just return the exact same permissions as you had on the node, if you have access via language
    private IEnumerable<string> GetLanguagePermissions<TVariant>(IContent content, MapperContext context, TVariant variantDisplay)
        where TVariant : ContentVariantDisplay
    {
        context.Items.TryGetValue("CurrentUser", out var currentBackofficeUser);

        IUser? currentUser = null;

        if (currentBackofficeUser is IUser currentIUserBackofficeUser)
        {
            currentUser = currentIUserBackofficeUser;
        }
        else if(_backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser is not null)
        {
            currentUser = _backOfficeSecurityAccessor.BackOfficeSecurity.CurrentUser;
        }

        if (currentUser is null)
        {
            return Enumerable.Empty<string>();
        }

        IEnumerable<IReadOnlyUserGroup> userGroups = currentUser.Groups;

        // Map allowed actions
        var hasAccess = false;
        foreach (IReadOnlyUserGroup group in userGroups)
        {
            // Handle invariant
            if (variantDisplay.Language is null)
            {
                var defaultLanguageId = _localizationService.GetDefaultLanguageId();
                if (_contentSettings.AllowEditInvariantFromNonDefault || (defaultLanguageId.HasValue && group.HasAccessToLanguage(defaultLanguageId.Value)))
                {
                    hasAccess = true;
                }
            }

            if (variantDisplay.Language is not null && group.HasAccessToLanguage(variantDisplay.Language.Id))
            {
                hasAccess = true;
                break;
            }
        }

        // If user does not have access, return only browse permission
        if (!hasAccess)
        {
            return new[] { ActionBrowse.ActionLetter.ToString() };
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
        {
            path = content.Path;
        }
        else
        {
            path = parent == null ? "-1" : parent.Path;
        }

        // A bit of a mess, but we need to ensure that all the required values are here AND that they're the right type.
        if (context.Items.TryGetValue("Permissions", out var permissionsObject) && permissionsObject is Dictionary<string, EntityPermissionSet> permissionsDict)
        {
            // If we already have permissions for a given path,
            // and the current user is the same as was used to generate the permissions, return the stored permissions.
            if (_backOfficeSecurityAccessor.BackOfficeSecurity?.CurrentUser?.Id == currentUser.Id &&
                permissionsDict.TryGetValue(path, out EntityPermissionSet? permissions))
            {
                return permissions.GetAllPermissions();
            }
        }

        // TODO: This is certainly not ideal usage here - perhaps the best way to deal with this in the future is
        // with the IUmbracoContextAccessor. In the meantime, if used outside of a web app this will throw a null
        // reference exception :(

        return _userService.GetPermissionsForPath(currentUser, path).GetAllPermissions();
    }
}
