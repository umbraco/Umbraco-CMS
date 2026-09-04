// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
///     Validates picked members against the member type filter a member picker is configured with.
/// </summary>
/// <remarks>
///     Shared by the member picker property editors, which differ in how many members they hold and so in the shape
///     of the value reaching validation, but not in what the filter means.
/// </remarks>
internal static class MemberTypeFilterValidator
{
    /// <summary>
    ///     Validates that every picked member exists and is of a member type the filter allows.
    /// </summary>
    /// <param name="memberKeys">The keys of the picked members.</param>
    /// <param name="configuration">The configuration holding the member type filter.</param>
    /// <param name="localizedTextService">The localized text service.</param>
    /// <param name="memberService">The member service.</param>
    /// <param name="coreScopeProvider">The core scope provider.</param>
    /// <returns>The validation results, which are empty when no filter is configured.</returns>
    public static IEnumerable<ValidationResult> Validate(
        IEnumerable<string>? memberKeys,
        MemberPickerConfigurationBase? configuration,
        ILocalizedTextService localizedTextService,
        IMemberService memberService,
        ICoreScopeProvider coreScopeProvider)
    {
        if (memberKeys is null || configuration is null)
        {
            return [];
        }

        HashSet<Guid> allowedMemberTypeKeys = AllowedContentTypeKeysParser.Parse(configuration.Filter);

        // No filter configured — all member types are allowed.
        if (allowedMemberTypeKeys.Count == 0)
        {
            return [];
        }

        Guid[] keys = memberKeys
            .Where(key => Guid.TryParse(key, out _))
            .Select(Guid.Parse)
            .Distinct()
            .ToArray();

        if (keys.Length == 0)
        {
            return [];
        }

        // The member service has no lookup by several keys, so the picked members are fetched one at a time. The
        // number of them is bounded by what an editor can pick.
        using ICoreScope scope = coreScopeProvider.CreateCoreScope();
        IMember?[] members = keys.Select(memberService.GetById).ToArray();
        scope.Complete();

        if (members.Any(member => member is null))
        {
            return
            [
                new ValidationResult(
                    localizedTextService.Localize("validation", "missingContent"),
                    ["value"])
            ];
        }

        if (members.WhereNotNull().Any(member => allowedMemberTypeKeys.Contains(member.ContentType.Key) is false))
        {
            return
            [
                new ValidationResult(
                    localizedTextService.Localize("validation", "invalidObjectType"),
                    ["value"])
            ];
        }

        return [];
    }
}
