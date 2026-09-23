using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class BlockElementResolver : IBlockElementResolver
{
    private readonly IContentService _contentService;
    private readonly IMediaService _mediaService;
    private readonly IMemberService _memberService;
    private readonly IElementService _elementService;
    private readonly IEntityService _entityService;
    private readonly IContentTypeService _contentTypeService;
    private readonly PropertyEditorCollection _propertyEditorCollection;
    private readonly IPropertyValidationService _propertyValidationService;
    private readonly ICultureImpactFactory _cultureImpactFactory;
    private readonly ILanguageService _languageService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockElementResolver" /> class.
    /// </summary>
    public BlockElementResolver(
        IContentService contentService,
        IMediaService mediaService,
        IMemberService memberService,
        IElementService elementService,
        IEntityService entityService,
        IContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        IPropertyValidationService propertyValidationService,
        ICultureImpactFactory cultureImpactFactory,
        ILanguageService languageService)
    {
        _contentService = contentService;
        _mediaService = mediaService;
        _memberService = memberService;
        _elementService = elementService;
        _entityService = entityService;
        _contentTypeService = contentTypeService;
        _propertyEditorCollection = propertyEditorCollection;
        _propertyValidationService = propertyValidationService;
        _cultureImpactFactory = cultureImpactFactory;
        _languageService = languageService;
    }

    /// <inheritdoc />
    public Attempt<BlockElementSource, ElementCreateFromBlockOperationStatus> Resolve(Guid ownerKey, Guid blockKey)
    {
        // The owner's type is resolved from its key rather than taken from the caller: node keys are unique
        // across object types, so the key alone says which service holds the owner.
        IEntitySlim? owner = _entityService.Get(ownerKey);
        if (owner is null)
        {
            return Fail(ElementCreateFromBlockOperationStatus.OwnerNotFound);
        }

        return ObjectTypes.GetUmbracoObjectType(owner.NodeObjectType) switch
        {
            UmbracoObjectTypes.Document => Resolve(_contentService, ownerKey, blockKey),
            UmbracoObjectTypes.Media => Resolve(_mediaService, ownerKey, blockKey),
            UmbracoObjectTypes.Member => Resolve(_memberService, ownerKey, blockKey),
            UmbracoObjectTypes.Element => Resolve(_elementService, ownerKey, blockKey),
            _ => Fail(ElementCreateFromBlockOperationStatus.OwnerTypeNotSupported),
        };
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<string?>> ResolvePublishableCulturesAsync(
        IElement element,
        IContentType elementType,
        IEnumerable<string?> liveCultures)
    {
        var elementVariesByCulture = elementType.VariesByCulture();
        ILanguage[] allLanguages = (await _languageService.GetAllAsync()).ToArray();

        // validate up front with the same validator the publish itself uses: it validates every culture
        // together, so one invalid culture would otherwise fail the publish for all of them.
        var validCultures = new List<string?>();
        foreach (string? culture in liveCultures)
        {
            CultureImpact impact = elementVariesByCulture
                ? _cultureImpactFactory.ImpactExplicit(culture, culture is not null && allLanguages.Any(l => l.IsoCode.InvariantEquals(culture) && l.IsMandatory))
                : _cultureImpactFactory.ImpactInvariant();

            if (_propertyValidationService.IsPropertyDataValid(element, out _, impact))
            {
                validCultures.Add(culture);
            }
        }

        // publishing a subset that leaves a mandatory culture uncovered unpublishes the whole element -
        // treat it as nothing publishable instead of letting that happen silently.
        return elementVariesByCulture && validCultures.Count > 0 && MandatoryCultureMissing(allLanguages, validCultures)
            ? []
            : validCultures;
    }

    private Attempt<BlockElementSource, ElementCreateFromBlockOperationStatus> Resolve<TItem>(
        IContentServiceBase<TItem> ownerService,
        Guid ownerKey,
        Guid blockKey)
        where TItem : class, IContentBase
    {
        TItem? owner = ownerService.GetById(ownerKey);
        if (owner is null)
        {
            return Fail(ElementCreateFromBlockOperationStatus.OwnerNotFound);
        }

        (IProperty Property, StoredBlock Block)? match = FindStoredBlockInAnyProperty(
            owner.Properties,
            _propertyEditorCollection,
            blockKey,
            owner is IPublishableContentBase);

        if (match is null)
        {
            return Fail(ElementCreateFromBlockOperationStatus.BlockNotFound);
        }

        (IProperty property, StoredBlock storedBlock) = match.Value;

        // every block on the path has a content type of its own, and each one's variance decides which culture
        // it has to be exposed in for the block inside it to be visible.
        IReadOnlyDictionary<Guid, IContentType> pathContentTypes = storedBlock.PublishedPath is not null
            ? _contentTypeService
                .GetMany(storedBlock.PublishedPath.Select(segment => segment.Content.ContentTypeKey).Distinct())
                .ToDictionary(contentType => contentType.Key)
            : new Dictionary<Guid, IContentType>();

        string?[] liveCultures = storedBlock.PublishedPath is not null
            ? ResolveLiveCultures(
                storedBlock.PublishedPath,
                owner.ContentType.Variations,
                property.PropertyType.VariesByCulture(),
                storedBlock.Culture,
                storedBlock.Segment,
                pathContentTypes)
            : [];

        return Attempt.SucceedWithStatus(
            ElementCreateFromBlockOperationStatus.Success,
            new BlockElementSource
            {
                ContentTypeKey = storedBlock.ContentTypeKey,
                DraftValues = storedBlock.Draft.Values.ToArray(),
                PublishedValues = storedBlock.Published?.Values.ToArray(),
                LiveCultures = liveCultures,
            });
    }

    /// <summary>
    ///     Finds the block among all of the owner's block properties, and the property holding it.
    /// </summary>
    /// <remarks>
    ///     Which property holds the block is found rather than taken from the caller: the block key alone
    ///     identifies it, and a block nested inside another sits in the value of the outermost property, which
    ///     a caller looking at the nested block has no way to name.
    ///     <para>
    ///     The property comes back with the block because its variance decides which cultures the block can be
    ///     live in - see <see cref="ResolveLiveCultures" />.
    ///     </para>
    /// </remarks>
    internal static (IProperty Property, StoredBlock Block)? FindStoredBlockInAnyProperty(
        IEnumerable<IProperty> properties,
        PropertyEditorCollection propertyEditors,
        Guid blockKey,
        bool ownerIsPublishable)
    {
        foreach (IProperty property in properties)
        {
            if (propertyEditors.TryGet(property.PropertyType.PropertyEditorAlias, out IDataEditor? dataEditor) is false
                || dataEditor.GetValueEditor() is not IBlockValueEditor blockValueEditor)
            {
                continue;
            }

            StoredBlock? block = FindStoredBlock(property, blockValueEditor, propertyEditors, blockKey, ownerIsPublishable);
            if (block is not null)
            {
                return (property, block);
            }
        }

        return null;
    }

    /// <summary>
    ///     A block as the owner currently has it stored, in both of the owner's versions.
    /// </summary>
    /// <param name="DraftPath">The blocks leading to it in the owner's draft, outermost first.</param>
    /// <param name="PublishedPath">The same in the owner's published version, or <c>null</c> when it is not there.</param>
    /// <param name="Culture">The culture of the property value slot the block was found in.</param>
    /// <param name="Segment">The segment of that slot.</param>
    internal sealed record StoredBlock(
        IReadOnlyList<BlockLevel> DraftPath,
        IReadOnlyList<BlockLevel>? PublishedPath,
        string? Culture,
        string? Segment)
    {
        public BlockItemData Draft => DraftPath[^1].Content;

        public BlockItemData? Published => PublishedPath?[^1].Content;

        public Guid ContentTypeKey => (Published ?? Draft).ContentTypeKey;
    }

    /// <summary>
    ///     Finds the stored property value slot that holds a block, in both of the owner's versions.
    /// </summary>
    /// <remarks>
    ///     Which slot the block lives in is found rather than taken from the caller: the block key alone
    ///     identifies it, and the slot that holds it is also what says which variation it lives in. Only one
    ///     slot can hold it - a property that varies keeps an independent block value per variation, and
    ///     blocks are only shared across variations within a single slot of an invariant property.
    ///     <para>
    ///     A non-publishable owner (media, member) has no published/draft split - its property values are simply
    ///     current, so the one stored value serves as both. A publishable owner's draft edits are never live:
    ///     only its published value is.
    ///     </para>
    /// </remarks>
    internal static StoredBlock? FindStoredBlock(
        IProperty property,
        IBlockValueEditor blockValueEditor,
        PropertyEditorCollection propertyEditors,
        Guid blockKey,
        bool ownerIsPublishable)
    {
        foreach (IPropertyValue propertyValue in property.Values)
        {
            IReadOnlyList<BlockLevel>? draftPath = FindBlock(blockValueEditor.GetBlockValue(propertyValue.EditedValue), propertyEditors, blockKey);
            IReadOnlyList<BlockLevel>? publishedPath = ownerIsPublishable
                ? FindBlock(blockValueEditor.GetBlockValue(propertyValue.PublishedValue), propertyEditors, blockKey)
                : draftPath;

            // a block present in the published version but removed from the draft is still readable, from the
            // version that has it.
            draftPath ??= publishedPath;
            if (draftPath is null)
            {
                continue;
            }

            return new StoredBlock(draftPath, publishedPath, propertyValue.Culture, propertyValue.Segment);
        }

        return null;
    }

    /// <summary>
    ///     One block on the way to the block sought, paired with the entries exposing it in the block value
    ///     that directly contains it.
    /// </summary>
    internal sealed record BlockLevel(BlockItemData Content, IReadOnlyList<BlockItemVariation> Expose);

    /// <summary>
    ///     Finds a block within a block value, descending into the block properties of other blocks.
    /// </summary>
    /// <returns>
    ///     The blocks leading to the one sought, outermost first and the block itself last, or <c>null</c> if
    ///     no block with that key is there at any depth.
    /// </returns>
    /// <remarks>
    ///     The whole chain is kept rather than just the block, because what is true of a block in context -
    ///     whether it is visible, and in which culture - depends on every block it sits inside.
    /// </remarks>
    private static IReadOnlyList<BlockLevel>? FindBlock(
        BlockValue? blockValue,
        PropertyEditorCollection propertyEditors,
        Guid blockKey)
    {
        if (blockValue is null)
        {
            return null;
        }

        var path = new List<BlockLevel>();
        return Descend(blockValue, propertyEditors, blockKey, path) ? path : null;
    }

    private static bool Descend(
        BlockValue blockValue,
        PropertyEditorCollection propertyEditors,
        Guid blockKey,
        List<BlockLevel> path)
    {
        BlockItemData? match = blockValue.ContentData.FirstOrDefault(x => x.Key == blockKey);
        if (match is not null)
        {
            path.Add(new BlockLevel(match, ExposeFor(blockValue, blockKey)));
            return true;
        }

        // a block's own properties can be block editors holding further blocks, so the search continues
        // through each of them, with that property's own editor doing the reading.
        foreach (BlockItemData content in blockValue.ContentData)
        {
            foreach (BlockPropertyValue propertyValue in content.Values)
            {
                BlockValue? nested = NestedBlockValue(propertyValue, propertyEditors);
                if (nested is null)
                {
                    continue;
                }

                path.Add(new BlockLevel(content, ExposeFor(blockValue, content.Key)));
                if (Descend(nested, propertyEditors, blockKey, path))
                {
                    return true;
                }

                path.RemoveAt(path.Count - 1);
            }
        }

        return false;
    }

    private static BlockValue? NestedBlockValue(BlockPropertyValue propertyValue, PropertyEditorCollection propertyEditors)
        => propertyValue.PropertyType is not null
           && propertyEditors.TryGet(propertyValue.PropertyType.PropertyEditorAlias, out IDataEditor? dataEditor)
           && dataEditor.GetValueEditor() is IBlockValueEditor nestedEditor
            ? nestedEditor.GetBlockValue(propertyValue.Value)
            : null;

    private static BlockItemVariation[] ExposeFor(BlockValue blockValue, Guid blockKey)
        => blockValue.Expose.Where(x => x.ContentKey == blockKey).ToArray();

    /// <summary>
    ///     Resolves the cultures a block is currently live in, given the blocks it sits inside.
    /// </summary>
    /// <param name="path">The blocks leading to it in the owner's published value, outermost first.</param>
    /// <param name="ownerVariations">The variations of the content type holding the property.</param>
    /// <param name="propertyVariesByCulture">Whether the property holding the outermost block varies by culture.</param>
    /// <param name="propertyCulture">The culture of the property value the block was found in.</param>
    /// <param name="propertySegment">The segment of that property value.</param>
    /// <param name="contentTypes">The content type of each block on the path, by key.</param>
    /// <returns>
    ///     The distinct live cultures, or a single <c>null</c> entry for an invariant block. Empty when the
    ///     block is not currently visible in any culture.
    /// </returns>
    /// <remarks>
    ///     A variant property already holds a separate value per culture, so a block found in it is only ever
    ///     live in that one property culture, regardless of what its own exposures say - culture splitting
    ///     already happened at the property level. An invariant property can hold blocks that vary
    ///     independently of it, so each exposed culture is a live culture in its own right.
    ///     <para>
    ///     A nested block is only as visible as the blocks it sits inside, so every one of them has to be
    ///     exposed too. Which variation each of them has to be exposed in is its own question: the variance
    ///     that applies at each step is the intersection of that block's content type with the one holding it.
    ///     This mirrors how rendering skips a block's whole subtree when the block itself is not exposed.
    ///     </para>
    /// </remarks>
    internal static string?[] ResolveLiveCultures(
        IReadOnlyList<BlockLevel> path,
        ContentVariation ownerVariations,
        bool propertyVariesByCulture,
        string? propertyCulture,
        string? propertySegment,
        IReadOnlyDictionary<Guid, IContentType> contentTypes)
    {
        string?[] candidates = propertyVariesByCulture
            ? [propertyCulture]
            : [.. path[0].Expose.Select(variation => variation.Culture).Distinct()];

        return [.. candidates.Where(culture => IsLive(path, ownerVariations, culture, propertySegment, contentTypes))];
    }

    /// <summary>
    ///     Gets a value indicating whether any mandatory language is absent from a set of cultures.
    /// </summary>
    /// <param name="languages">All configured languages.</param>
    /// <param name="cultures">The cultures about to be published.</param>
    /// <remarks>
    ///     Publishing a subset of cultures that does not cover every mandatory language unpublishes the whole
    ///     item, so this is what stops a partial publish from silently taking the element offline.
    /// </remarks>
    internal static bool MandatoryCultureMissing(IEnumerable<ILanguage> languages, IReadOnlyCollection<string?> cultures)
        => languages
            .Where(language => language.IsMandatory)
            .Any(language => cultures.Any(culture => culture is not null && culture.InvariantEquals(language.IsoCode)) is false);

    private static bool IsLive(
        IReadOnlyList<BlockLevel> path,
        ContentVariation ownerVariations,
        string? culture,
        string? segment,
        IReadOnlyDictionary<Guid, IContentType> contentTypes)
    {
        foreach (BlockLevel step in path)
        {
            if (contentTypes.TryGetValue(step.Content.ContentTypeKey, out IContentType? blockType) is false)
            {
                return false;
            }

            // the variance that applies to a block is its own intersected with the one holding it, for each
            // axis independently - the same rule rendering applies.
            ContentVariation exposeVariation = ownerVariations & blockType.Variations;
            var expectedCulture = exposeVariation.VariesByCulture() ? culture : null;
            var expectedSegment = exposeVariation.VariesBySegment() ? segment : null;

            // deliberately no language fallback, unlike rendering: this reproduces what is actually
            // published, not what a fallback policy happens to surface.
            var exposed = step.Expose.Any(variation =>
                variation.Culture.InvariantEquals(expectedCulture) &&
                variation.Segment == expectedSegment);

            if (exposed is false)
            {
                return false;
            }

            ownerVariations = blockType.Variations;
        }

        return true;
    }

    private static Attempt<BlockElementSource, ElementCreateFromBlockOperationStatus> Fail(ElementCreateFromBlockOperationStatus status)
        => Attempt.FailWithStatus(
            status,
            new BlockElementSource
            {
                ContentTypeKey = Guid.Empty,
                DraftValues = [],
                LiveCultures = [],
            });
}
