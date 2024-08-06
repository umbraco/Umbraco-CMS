// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Cache.PropertyEditors;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;
using BlockGridAreaConfiguration = Umbraco.Cms.Core.PropertyEditors.BlockGridConfiguration.BlockGridAreaConfiguration;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Abstract base class for block grid based editors.
/// </summary>
public abstract class BlockGridPropertyEditorBase : DataEditor
{
    private readonly IBlockValuePropertyIndexValueFactory _blockValuePropertyIndexValueFactory;

    [Obsolete("Use non-obsoleted ctor. This will be removed in Umbraco 13.")]
    protected BlockGridPropertyEditorBase(IDataValueEditorFactory dataValueEditorFactory)
        : this(dataValueEditorFactory, StaticServiceProvider.Instance.GetRequiredService<IBlockValuePropertyIndexValueFactory>())
    {

    }

    protected BlockGridPropertyEditorBase(IDataValueEditorFactory dataValueEditorFactory, IBlockValuePropertyIndexValueFactory blockValuePropertyIndexValueFactory)
        : base(dataValueEditorFactory)
    {
        _blockValuePropertyIndexValueFactory = blockValuePropertyIndexValueFactory;
        SupportsReadOnly = true;
    }

    public override IPropertyIndexValueFactory PropertyIndexValueFactory => _blockValuePropertyIndexValueFactory;


    #region Value Editor

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<BlockGridEditorPropertyValueEditor>(Attribute!);

    private class BlockGridEditorPropertyValueEditor : BlockEditorPropertyValueEditor
    {
        public BlockGridEditorPropertyValueEditor(
            DataEditorAttribute attribute,
            PropertyEditorCollection propertyEditors,
            DataValueReferenceFactoryCollection dataValueReferenceFactories,
            IDataTypeConfigurationCache dataTypeConfigurationCache,
            ILocalizedTextService textService,
            ILogger<BlockEditorPropertyValueEditor> logger,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            IBlockEditorElementTypeCache elementTypeCache,
            IPropertyValidationService propertyValidationService)
            : base(attribute, propertyEditors, dataValueReferenceFactories, dataTypeConfigurationCache, textService, logger, shortStringHelper, jsonSerializer, ioHelper)
        {
            BlockEditorValues = new BlockEditorValues(new BlockGridEditorDataConverter(jsonSerializer), elementTypeCache, logger);
            Validators.Add(new BlockEditorValidator(propertyValidationService, BlockEditorValues, elementTypeCache));
            Validators.Add(new MinMaxValidator(BlockEditorValues, textService));
        }

        private class MinMaxValidator : BlockEditorMinMaxValidatorBase
        {
            private readonly BlockEditorValues _blockEditorValues;

            public MinMaxValidator(BlockEditorValues blockEditorValues, ILocalizedTextService textService)
                : base(textService) =>
                _blockEditorValues = blockEditorValues;

            public override IEnumerable<ValidationResult> Validate(object? value, string? valueType, object? dataTypeConfiguration)
            {
                if (dataTypeConfiguration is not BlockGridConfiguration blockConfig)
                {
                    return Array.Empty<ValidationResult>();
                }

                BlockEditorData? blockEditorData = _blockEditorValues.DeserializeAndClean(value);

                var validationResults = new List<ValidationResult>();
                validationResults.AddRange(ValidateNumberOfBlocks(blockEditorData, blockConfig.ValidationLimit.Min, blockConfig.ValidationLimit.Max));

                var areasConfigsByKey = blockConfig.Blocks.SelectMany(b => b.Areas).ToDictionary(a => a.Key);

                IList<BlockGridLayoutAreaItem> ExtractLayoutAreaItems(BlockGridLayoutItem item)
                {
                    var areas = item.Areas.ToList();
                    areas.AddRange(item.Areas.SelectMany(a => a.Items).SelectMany(ExtractLayoutAreaItems));
                    return areas;
                }

                BlockGridLayoutAreaItem[]? areas = blockEditorData?.Layout?.ToObject<IEnumerable<BlockGridLayoutItem>>()?.SelectMany(ExtractLayoutAreaItems).ToArray();

                if (areas?.Any() != true)
                {
                    return validationResults;
                }

                foreach (BlockGridLayoutAreaItem area in areas)
                {
                    if (!areasConfigsByKey.TryGetValue(area.Key, out BlockGridAreaConfiguration? areaConfig))
                    {
                        continue;
                    }

                    if ((areaConfig.MinAllowed.HasValue && area.Items.Length < areaConfig.MinAllowed) || (areaConfig.MaxAllowed.HasValue && area.Items.Length > areaConfig.MaxAllowed))
                    {
                        validationResults.Add(new ValidationResult(TextService.Localize("validation", "entriesAreasMismatch")));
                    }
                }

                return validationResults;
            }
        }
    }

    #endregion
}
