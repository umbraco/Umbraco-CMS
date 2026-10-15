// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Umbraco.Cms.Core.Models.Validation;
using Umbraco.Cms.Core.PropertyEditors.Validation;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.Validators;

/// <summary>
///     Validates the members picked by an editor holding any number of them against its member type filter.
/// </summary>
internal sealed class MultipleMemberTypeFilterValidator : ITypedValidator<List<string>, MemberPickerConfigurationBase>
{
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IMemberService _memberService;
    private readonly ICoreScopeProvider _coreScopeProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MultipleMemberTypeFilterValidator" /> class.
    /// </summary>
    /// <param name="localizedTextService">The localized text service.</param>
    /// <param name="memberService">The member service.</param>
    /// <param name="coreScopeProvider">The core scope provider.</param>
    public MultipleMemberTypeFilterValidator(
        ILocalizedTextService localizedTextService,
        IMemberService memberService,
        ICoreScopeProvider coreScopeProvider)
    {
        _localizedTextService = localizedTextService;
        _memberService = memberService;
        _coreScopeProvider = coreScopeProvider;
    }

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(
        List<string>? value,
        MemberPickerConfigurationBase? configuration,
        string? valueType,
        PropertyValidationContext validationContext)
        => MemberTypeFilterValidator.Validate(
            value,
            configuration,
            _localizedTextService,
            _memberService,
            _coreScopeProvider);
}
