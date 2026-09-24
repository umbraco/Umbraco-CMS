using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.Services;

/// <inheritdoc />
internal sealed class BlockElementResolver : IBlockElementResolver
{
    private readonly IContentService _contentService;
    private readonly IMediaService _mediaService;
    private readonly IMemberService _memberService;
    private readonly IElementService _elementService;
    private readonly IEntityService _entityService;
    private readonly PropertyEditorCollection _propertyEditorCollection;

    /// <summary>
    ///     Initializes a new instance of the <see cref="BlockElementResolver" /> class.
    /// </summary>
    public BlockElementResolver(
        IContentService contentService,
        IMediaService mediaService,
        IMemberService memberService,
        IElementService elementService,
        IEntityService entityService,
        PropertyEditorCollection propertyEditorCollection)
    {
        _contentService = contentService;
        _mediaService = mediaService;
        _memberService = memberService;
        _elementService = elementService;
        _entityService = entityService;
        _propertyEditorCollection = propertyEditorCollection;
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

        BlockItemData? block = FindStoredBlockInAnyProperty(owner.Properties, _propertyEditorCollection, blockKey);
        if (block is null)
        {
            return Fail(ElementCreateFromBlockOperationStatus.BlockNotFound);
        }

        return Attempt.SucceedWithStatus(
            ElementCreateFromBlockOperationStatus.Success,
            new BlockElementSource
            {
                ContentTypeKey = block.ContentTypeKey,
                Values = block.Values.ToArray(),
            });
    }

    /// <summary>
    ///     Finds a block among all of a content item's block properties.
    /// </summary>
    /// <remarks>
    ///     Which property holds the block is worked out here rather than named by the caller: the block key
    ///     alone identifies it, and a block nested inside another sits in the value of the outermost property,
    ///     which a caller looking at the nested block has no way to name.
    /// </remarks>
    internal static BlockItemData? FindStoredBlockInAnyProperty(
        IEnumerable<IProperty> properties,
        PropertyEditorCollection propertyEditors,
        Guid blockKey)
    {
        foreach (IProperty property in properties)
        {
            if (propertyEditors.TryGet(property.PropertyType.PropertyEditorAlias, out IDataEditor? dataEditor) is false
                || dataEditor.GetValueEditor() is not IBlockValueEditor blockValueEditor)
            {
                continue;
            }

            BlockItemData? block = FindStoredBlock(property, blockValueEditor, propertyEditors, blockKey);
            if (block is not null)
            {
                return block;
            }
        }

        return null;
    }

    /// <summary>
    ///     Finds a block in whichever of a property's stored value slots holds it.
    /// </summary>
    /// <remarks>
    ///     A property that varies keeps an independent block value per variation, so the block lives in exactly
    ///     one slot and which one is found rather than taken from the caller.
    ///     <para>
    ///     Only the edited value is read. That is what the editor is looking at, and for a media item or member
    ///     it is the only value there is.
    ///     </para>
    /// </remarks>
    internal static BlockItemData? FindStoredBlock(
        IProperty property,
        IBlockValueEditor blockValueEditor,
        PropertyEditorCollection propertyEditors,
        Guid blockKey)
    {
        foreach (IPropertyValue propertyValue in property.Values)
        {
            BlockItemData? block = FindBlock(blockValueEditor.GetBlockValue(propertyValue.EditedValue), propertyEditors, blockKey);
            if (block is not null)
            {
                return block;
            }
        }

        return null;
    }

    private static BlockItemData? FindBlock(
        BlockValue? blockValue,
        PropertyEditorCollection propertyEditors,
        Guid blockKey)
    {
        if (blockValue is null)
        {
            return null;
        }

        BlockItemData? match = blockValue.ContentData.FirstOrDefault(x => x.Key == blockKey);
        if (match is not null)
        {
            return match;
        }

        // a block's own properties can be block editors holding further blocks, so the search continues
        // through each of them, with that property's own editor doing the reading.
        foreach (BlockItemData content in blockValue.ContentData)
        {
            foreach (BlockPropertyValue propertyValue in content.Values)
            {
                BlockValue? nested = NestedBlockValue(propertyValue, propertyEditors);
                BlockItemData? nestedMatch = nested is null ? null : FindBlock(nested, propertyEditors, blockKey);
                if (nestedMatch is not null)
                {
                    return nestedMatch;
                }
            }
        }

        return null;
    }

    private static BlockValue? NestedBlockValue(BlockPropertyValue propertyValue, PropertyEditorCollection propertyEditors)
        => propertyValue.PropertyType is not null
           && propertyEditors.TryGet(propertyValue.PropertyType.PropertyEditorAlias, out IDataEditor? dataEditor)
           && dataEditor.GetValueEditor() is IBlockValueEditor nestedEditor
            ? nestedEditor.GetBlockValue(propertyValue.Value)
            : null;

    private static Attempt<BlockElementSource, ElementCreateFromBlockOperationStatus> Fail(ElementCreateFromBlockOperationStatus status)
        => Attempt.FailWithStatus(
            status,
            new BlockElementSource
            {
                ContentTypeKey = Guid.Empty,
                Values = [],
            });
}
