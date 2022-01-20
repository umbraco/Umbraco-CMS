﻿// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Templates;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Represents a grid property and parameter editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.Grid,
        "Grid layout",
        "grid",
        HideLabel = true,
        ValueType = ValueTypes.Json,
        Icon = "icon-layout",
        Group = Constants.PropertyEditors.Groups.RichContent)]
    public class GridPropertyEditor : DataEditor
    {
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly IIOHelper _ioHelper;
        private readonly HtmlImageSourceParser _imageSourceParser;
        private readonly RichTextEditorPastedImages _pastedImages;
        private readonly HtmlLocalLinkParser _localLinkParser;
        private readonly IImageUrlGenerator _imageUrlGenerator;

        public GridPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            HtmlImageSourceParser imageSourceParser,
            RichTextEditorPastedImages pastedImages,
            HtmlLocalLinkParser localLinkParser,
            IIOHelper ioHelper,
            IImageUrlGenerator imageUrlGenerator)
            : base(dataValueEditorFactory)
        {
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
            _ioHelper = ioHelper;
            _imageSourceParser = imageSourceParser;
            _pastedImages = pastedImages;
            _localLinkParser = localLinkParser;
            _imageUrlGenerator = imageUrlGenerator;
        }

        public override IPropertyIndexValueFactory PropertyIndexValueFactory => new GridPropertyIndexValueFactory();

        /// <summary>
        /// Overridden to ensure that the value is validated
        /// </summary>
        /// <returns></returns>
        protected override IDataValueEditor CreateValueEditor() => DataValueEditorFactory.Create<GridPropertyValueEditor>(Attribute);

        protected override IConfigurationEditor CreateConfigurationEditor() => new GridConfigurationEditor(_ioHelper);

        internal class GridPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
            private readonly HtmlImageSourceParser _imageSourceParser;
            private readonly RichTextEditorPastedImages _pastedImages;
            private readonly RichTextPropertyEditor.RichTextPropertyValueEditor _richTextPropertyValueEditor;
            private readonly MediaPickerPropertyEditor.MediaPickerPropertyValueEditor _mediaPickerPropertyValueEditor;
            private readonly IImageUrlGenerator _imageUrlGenerator;

            public GridPropertyValueEditor(
                IDataValueEditorFactory dataValueEditorFactory,
                DataEditorAttribute attribute,
                IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
                ILocalizedTextService localizedTextService,
                HtmlImageSourceParser imageSourceParser,
                RichTextEditorPastedImages pastedImages,
                IShortStringHelper shortStringHelper,
                IImageUrlGenerator imageUrlGenerator,
                IJsonSerializer jsonSerializer,
                IIOHelper ioHelper)
                : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
            {
                _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
                _imageSourceParser = imageSourceParser;
                _pastedImages = pastedImages;
                _richTextPropertyValueEditor =
                    dataValueEditorFactory.Create<RichTextPropertyEditor.RichTextPropertyValueEditor>(attribute);
                _mediaPickerPropertyValueEditor =
                    dataValueEditorFactory.Create<MediaPickerPropertyEditor.MediaPickerPropertyValueEditor>(attribute);
                _imageUrlGenerator = imageUrlGenerator;
            }

            /// <summary>
            /// Format the data for persistence
            /// This to ensure if a RTE is used in a Grid cell/control that we parse it for tmp stored images
            /// to persist to the media library when we go to persist this to the DB
            /// </summary>
            /// <param name="editorValue"></param>
            /// <param name="currentValue"></param>
            /// <returns></returns>
            public override object FromEditor(ContentPropertyData editorValue, object currentValue)
            {
                if (editorValue.Value == null)
                    return null;

                // editorValue.Value is a JSON string of the grid
                var rawJson = editorValue.Value.ToString();
                if (rawJson.IsNullOrWhiteSpace())
                    return null;

                var config = editorValue.DataTypeConfiguration as GridConfiguration;
                var mediaParent = config?.MediaParentId;
                var mediaParentId = mediaParent == null ? Guid.Empty : mediaParent.Guid;

                var grid = DeserializeGridValue(rawJson, out var rtes, out _);

                var userId = _backOfficeSecurityAccessor?.BackOfficeSecurity?.CurrentUser?.Id ?? Constants.Security.SuperUserId;

                // Process the rte values
                foreach (var rte in rtes)
                {
                    // Parse the HTML
                    var html = rte.Value?.ToString();

                    var parseAndSavedTempImages = _pastedImages.FindAndPersistPastedTempImages(html, mediaParentId, userId, _imageUrlGenerator);
                    var editorValueWithMediaUrlsRemoved = _imageSourceParser.RemoveImageSources(parseAndSavedTempImages);

                    rte.Value = editorValueWithMediaUrlsRemoved;
                }

                // Convert back to raw JSON for persisting
                return JsonConvert.SerializeObject(grid);
            }

            /// <summary>
            /// Ensures that the rich text editor values are processed within the grid
            /// </summary>
            /// <param name="property"></param>
            /// <param name="dataTypeService"></param>
            /// <param name="culture"></param>
            /// <param name="segment"></param>
            /// <returns></returns>
            public override object ToEditor(IProperty property, string culture = null, string segment = null)
            {
                var val = property.GetValue(culture, segment)?.ToString();
                if (val.IsNullOrWhiteSpace()) return string.Empty;

                var grid = DeserializeGridValue(val, out var rtes, out _);

                //process the rte values
                foreach (var rte in rtes.ToList())
                {
                    var html = rte.Value?.ToString();

                    var propertyValueWithMediaResolved = _imageSourceParser.EnsureImageSources(html);
                    rte.Value = propertyValueWithMediaResolved;
                }

                return grid;
            }

            private GridValue DeserializeGridValue(string rawJson, out IEnumerable<GridValue.GridControl> richTextValues, out IEnumerable<GridValue.GridControl> mediaValues)
            {
                var grid = JsonConvert.DeserializeObject<GridValue>(rawJson);

                // Find all controls that use the RTE editor
                var controls = grid.Sections.SelectMany(x => x.Rows.SelectMany(r => r.Areas).SelectMany(a => a.Controls)).ToArray();
                richTextValues = controls.Where(x => x.Editor.Alias.ToLowerInvariant() == "rte");
                mediaValues = controls.Where(x => x.Editor.Alias.ToLowerInvariant() == "media");

                return grid;
            }

            /// <summary>
            /// Resolve references from <see cref="IDataValueEditor"/> values
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public IEnumerable<UmbracoEntityReference> GetReferences(object value)
            {
                var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

                if (rawJson.IsNullOrWhiteSpace())
                    yield break;

                DeserializeGridValue(rawJson, out var richTextEditorValues, out var mediaValues);

                foreach (var umbracoEntityReference in richTextEditorValues.SelectMany(x =>
                    _richTextPropertyValueEditor.GetReferences(x.Value)))
                    yield return umbracoEntityReference;

                foreach (var umbracoEntityReference in mediaValues.Where(x=>x.Value.HasValues)
                    .SelectMany(x => _mediaPickerPropertyValueEditor.GetReferences(x.Value["udi"])))
                    yield return umbracoEntityReference;
            }
        }
    }
}
