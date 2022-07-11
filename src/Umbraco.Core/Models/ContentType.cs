using System.Runtime.Serialization;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Strings;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents the content type that a <see cref="Content" /> object is based on
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class ContentType : ContentTypeCompositionBase, IContentTypeWithHistoryCleanup
{
    public const bool SupportsPublishingConst = true;

    // Custom comparer for enumerable
    private static readonly DelegateEqualityComparer<IEnumerable<ITemplate>> TemplateComparer = new(
        (templates, enumerable) => templates.UnsortedSequenceEqual(enumerable),
        templates => templates.GetHashCode());

    private IEnumerable<ITemplate>? _allowedTemplates;

    private int _defaultTemplate;

    private HistoryCleanup? _historyCleanup;

    /// <summary>
    ///     Constuctor for creating a ContentType with the parent's id.
    /// </summary>
    /// <remarks>Only use this for creating ContentTypes at the root (with ParentId -1).</remarks>
    /// <param name="parentId"></param>
    /// <param name="shortStringHelper"></param>
    public ContentType(IShortStringHelper shortStringHelper, int parentId)
        : base(shortStringHelper, parentId)
    {
        _allowedTemplates = new List<ITemplate>();
        HistoryCleanup = new HistoryCleanup();
    }

    /// <summary>
    ///     Constuctor for creating a ContentType with the parent as an inherited type.
    /// </summary>
    /// <remarks>Use this to ensure inheritance from parent.</remarks>
    /// <param name="parent"></param>
    /// <param name="alias"></param>
    /// <param name="shortStringHelper"></param>
    public ContentType(IShortStringHelper shortStringHelper, IContentType parent, string alias)
        : base(shortStringHelper, parent, alias)
    {
        _allowedTemplates = new List<ITemplate>();
        HistoryCleanup = new HistoryCleanup();
    }

    /// <inheritdoc />
    public override bool SupportsPublishing => SupportsPublishingConst;

    /// <summary>
    ///     Gets or sets the alias of the default Template.
    ///     TODO: This should be ignored from cloning!!!!!!!!!!!!!!
    ///     - but to do that we have to implement callback hacks, this needs to be fixed in v8,
    ///     we should not store direct entity
    /// </summary>
    [IgnoreDataMember]
    public ITemplate? DefaultTemplate =>
        AllowedTemplates?.FirstOrDefault(x => x != null && x.Id == DefaultTemplateId);

    /// <inheritdoc />
    public override ISimpleContentType ToSimple() => new SimpleContentType(this);

    [DataMember]
    public int DefaultTemplateId
    {
        get => _defaultTemplate;
        set => SetPropertyValueAndDetectChanges(value, ref _defaultTemplate, nameof(DefaultTemplateId));
    }

    /// <summary>
    ///     Gets or Sets a list of Templates which are allowed for the ContentType
    ///     TODO: This should be ignored from cloning!!!!!!!!!!!!!!
    ///     - but to do that we have to implement callback hacks, this needs to be fixed in v8,
    ///     we should not store direct entity
    /// </summary>
    [DataMember]
    public IEnumerable<ITemplate>? AllowedTemplates
    {
        get => _allowedTemplates;
        set
        {
            SetPropertyValueAndDetectChanges(value, ref _allowedTemplates, nameof(AllowedTemplates), TemplateComparer);

            if (_allowedTemplates?.Any(x => x.Id == _defaultTemplate) == false)
            {
                DefaultTemplateId = 0;
            }
        }
    }

    public HistoryCleanup? HistoryCleanup
    {
        get => _historyCleanup;
        set => SetPropertyValueAndDetectChanges(value, ref _historyCleanup, nameof(HistoryCleanup));
    }

    /// <summary>
    ///     Determines if AllowedTemplates contains templateId
    /// </summary>
    /// <param name="templateId">The template id to check</param>
    /// <returns>True if AllowedTemplates contains the templateId else False</returns>
    public bool IsAllowedTemplate(int templateId) =>
        AllowedTemplates == null
            ? false
            : AllowedTemplates.Any(t => t.Id == templateId);

    /// <summary>
    ///     Determines if AllowedTemplates contains templateId
    /// </summary>
    /// <param name="templateAlias">The template alias to check</param>
    /// <returns>True if AllowedTemplates contains the templateAlias else False</returns>
    public bool IsAllowedTemplate(string templateAlias) =>
        AllowedTemplates == null
            ? false
            : AllowedTemplates.Any(t => t.Alias.Equals(templateAlias, StringComparison.InvariantCultureIgnoreCase));

    /// <summary>
    ///     Sets the default template for the ContentType
    /// </summary>
    /// <param name="template">Default <see cref="ITemplate" /></param>
    public void SetDefaultTemplate(ITemplate? template)
    {
        if (template == null)
        {
            DefaultTemplateId = 0;
            return;
        }

        DefaultTemplateId = template.Id;
        if (_allowedTemplates?.Any(x => x != null && x.Id == template.Id) == false)
        {
            var templates = AllowedTemplates?.ToList();
            templates?.Add(template);
            AllowedTemplates = templates;
        }
    }

    /// <summary>
    ///     Removes a template from the list of allowed templates
    /// </summary>
    /// <param name="template"><see cref="ITemplate" /> to remove</param>
    /// <returns>True if template was removed, otherwise False</returns>
    public bool RemoveTemplate(ITemplate template)
    {
        if (DefaultTemplateId == template.Id)
        {
            DefaultTemplateId = default;
        }

        var templates = AllowedTemplates?.ToList();
        ITemplate? remove = templates?.FirstOrDefault(x => x.Id == template.Id);
        var result = remove is not null && templates is not null && templates.Remove(remove);
        AllowedTemplates = templates;

        return result;
    }

    /// <inheritdoc />
    IContentType IContentType.DeepCloneWithResetIdentities(string newAlias) =>
        (IContentType)DeepCloneWithResetIdentities(newAlias);

    /// <inheritdoc />
    public override bool IsDirty() => base.IsDirty() || (HistoryCleanup?.IsDirty() ?? false);
}
