// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Defines an email address property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.EmailAddress,
    ValueEditorIsReusable = true)]
public class EmailAddressPropertyEditor : DataEditor
{
    private readonly ILocalizedTextService _localizedTextService;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddressPropertyEditor"/> class.
    /// </summary>
    [Obsolete("Please use the constructor taking all parameters. Scheduled for removal in Umbraco 17.")]
    public EmailAddressPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : this(
              dataValueEditorFactory,
              StaticServiceProvider.Instance.GetRequiredService<ILocalizedTextService>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailAddressPropertyEditor"/> class.
    /// </summary>
    public EmailAddressPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, ILocalizedTextService localizedTextService)
        : base(dataValueEditorFactory)
    {
        SupportsReadOnly = true;
        _localizedTextService = localizedTextService;
    }

    /// <inheritdoc/>
    protected override IDataValueEditor CreateValueEditor()
    {
        IDataValueEditor editor = base.CreateValueEditor();
        editor.Validators.Add(new EmailValidator(_localizedTextService));
        return editor;
    }
}
