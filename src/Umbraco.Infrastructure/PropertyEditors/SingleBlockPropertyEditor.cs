using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Infrastructure.PropertyEditors;

/// <summary>
/// Represents a single block property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.SingleBlock,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = false)] //todo check reusability
public class SingleBlockPropertyEditor : DataEditor
{
    private readonly IJsonSerializer _jsonSerializer;

    public SingleBlockPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IJsonSerializer jsonSerializer)
        : base(dataValueEditorFactory)
    {
        _jsonSerializer = jsonSerializer;
    }

    // todo
    // public override IPropertyIndexValueFactory PropertyIndexValueFactory => _blockValuePropertyIndexValueFactory;

    /// <inheritdoc/>
    public override bool SupportsConfigurableElements => true;

    /// <summary>
    /// Instantiates a new <see cref="BlockEditorDataConverter{SingleBlockValue, SingleBlockLayoutItem}"/> for use with the single block editor property value editor.
    /// </summary>
    /// <returns>A new instance of <see cref="SingleBlockEditorDataConverter"/>.</returns>
    protected virtual BlockEditorDataConverter<SingleBlockValue, SingleBlockLayoutItem> CreateBlockEditorDataConverter()
        => new SingleBlockEditorDataConverter(_jsonSerializer);

    /// <inheritdoc/>
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<SingleBlockEditorPropertyValueEditor>(Attribute!, CreateBlockEditorDataConverter());

    //todo check CanMergePartialPropertyValues & MergePartialPropertyValueForCulture

    internal sealed class SingleBlockEditorPropertyValueEditor : BlockEditorPropertyValueEditor<SingleBlockValue, SingleBlockLayoutItem>
    {
        public SingleBlockEditorPropertyValueEditor(
            DataEditorAttribute attribute,
            BlockEditorDataConverter<SingleBlockValue, SingleBlockLayoutItem> blockEditorDataConverter,
            PropertyEditorCollection propertyEditors,
            DataValueReferenceFactoryCollection dataValueReferenceFactories,
            IDataTypeConfigurationCache dataTypeConfigurationCache,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            BlockEditorVarianceHandler blockEditorVarianceHandler,
            ILanguageService languageService,
            IIOHelper ioHelper,
            IBlockEditorElementTypeCache elementTypeCache,
            ILogger<SingleBlockEditorPropertyValueEditor> logger)
            : base(propertyEditors, dataValueReferenceFactories, dataTypeConfigurationCache, shortStringHelper, jsonSerializer, blockEditorVarianceHandler, languageService, ioHelper, attribute)
        {
            BlockEditorValues = new BlockEditorValues<SingleBlockValue, SingleBlockLayoutItem>(blockEditorDataConverter, elementTypeCache, logger);
            //todo check blocklistpropertyeditorbase constructor
        }

        protected override SingleBlockValue CreateWithLayout(IEnumerable<SingleBlockLayoutItem> layout) =>
            new(layout.Single());

        /// <inheritdoc/>
        public override IEnumerable<Guid> ConfiguredElementTypeKeys()
        {
            var configuration = ConfigurationObject as SingleBlockConfiguration;
            return configuration?.Blocks.SelectMany(ConfiguredElementTypeKeys) ?? Enumerable.Empty<Guid>();
        }
    }
}
