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
///     Validates the member picked by an editor holding a single member against its member type filter.
/// </summary>
internal sealed class SingleMemberTypeFilterValidator : ITypedValidator<string, MemberPickerConfigurationBase>
{
    private readonly ILocalizedTextService _localizedTextService;
    private readonly IMemberService _memberService;
    private readonly ICoreScopeProvider _coreScopeProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SingleMemberTypeFilterValidator" /> class.
    /// </summary>
    /// <param name="localizedTextService">The localized text service.</param>
    /// <param name="memberService">The member service.</param>
    /// <param name="coreScopeProvider">The core scope provider.</param>
    public SingleMemberTypeFilterValidator(
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
        string? value,
        MemberPickerConfigurationBase? configuration,
        string? valueType,
        PropertyValidationContext validationContext)
        => MemberTypeFilterValidator.Validate(
            value.IsNullOrWhiteSpace() ? null : [value],
            configuration,
            _localizedTextService,
            _memberService,
            _coreScopeProvider);
}
