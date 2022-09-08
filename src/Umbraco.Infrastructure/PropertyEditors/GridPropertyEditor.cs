// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Media;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Infrastructure.Templates;
using Umbraco.Cms.Web.Common.DependencyInjection;
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
        Group = Constants.PropertyEditors.Groups.RichContent,
        ValueEditorIsReusable = false)]
    public class GridPropertyEditor : DataEditor
    {
        private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
        private readonly IIOHelper _ioHelper;
        private readonly HtmlImageSourceParser _imageSourceParser;
        private readonly RichTextEditorPastedImages _pastedImages;
        private readonly HtmlLocalLinkParser _localLinkParser;
        private readonly IImageUrlGenerator _imageUrlGenerator;
        private readonly IHtmlMacroParameterParser _macroParameterParser;
        private readonly IEditorConfigurationParser _editorConfigurationParser;

        // Scheduled for removal in v12
        [Obsolete("Use the constructor which takes an IHtmlMacroParameterParser instead")]
        public GridPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            HtmlImageSourceParser imageSourceParser,
            RichTextEditorPastedImages pastedImages,
            HtmlLocalLinkParser localLinkParser,
            IIOHelper ioHelper,
            IImageUrlGenerator imageUrlGenerator,
            IHtmlMacroParameterParser macroParameterParser)
            : this(
                dataValueEditorFactory,
                backOfficeSecurityAccessor,
                imageSourceParser,
                pastedImages,
                localLinkParser,
                ioHelper,
                imageUrlGenerator,
                macroParameterParser,
                StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
        {
        }

        [Obsolete("Use the constructor which takes an IHtmlMacroParameterParser instead")]
        public GridPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            HtmlImageSourceParser imageSourceParser,
            RichTextEditorPastedImages pastedImages,
            HtmlLocalLinkParser localLinkParser,
            IIOHelper ioHelper,
            IImageUrlGenerator imageUrlGenerator)
            : this(
                dataValueEditorFactory,
                backOfficeSecurityAccessor,
                imageSourceParser,
                pastedImages,
                localLinkParser,
                ioHelper,
                imageUrlGenerator,
                StaticServiceProvider.Instance.GetRequiredService<IHtmlMacroParameterParser>(),
                StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
        {
        }

        public GridPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IBackOfficeSecurityAccessor backOfficeSecurityAccessor,
            HtmlImageSourceParser imageSourceParser,
            RichTextEditorPastedImages pastedImages,
            HtmlLocalLinkParser localLinkParser,
            IIOHelper ioHelper,
            IImageUrlGenerator imageUrlGenerator,
            IHtmlMacroParameterParser macroParameterParser,
            IEditorConfigurationParser editorConfigurationParser)
            : base(dataValueEditorFactory)
        {
            _backOfficeSecurityAccessor = backOfficeSecurityAccessor;
            _ioHelper = ioHelper;
            _imageSourceParser = imageSourceParser;
            _pastedImages = pastedImages;
            _localLinkParser = localLinkParser;
            _imageUrlGenerator = imageUrlGenerator;
            _macroParameterParser = macroParameterParser;
            _editorConfigurationParser = editorConfigurationParser;
            SupportsReadOnly = true;
        }

        public override IPropertyIndexValueFactory PropertyIndexValueFactory => new GridPropertyIndexValueFactory();

        /// <summary>
        /// Overridden to ensure that the value is validated
        /// </summary>
        /// <returns></returns>
        protected override IDataValueEditor CreateValueEditor() => DataValueEditorFactory.Create<GridPropertyValueEditor>(Attribute!);

        protected override IConfigurationEditor CreateConfigurationEditor() => new GridConfigurationEditor(_ioHelper, _editorConfigurationParser);

        internal class GridPropertyValueEditor : DataValueEditor, IDataValueReference
        {
            private readonly IBackOfficeSecurityAccessor _backOfficeSecurityAccessor;
            private readonly HtmlImageSourceParser _imageSourceParser;
            private readonly RichTextEditorPastedImages _pastedImages;
            private readonly RichTextPropertyEditor.RichTextPropertyValueEditor _richTextPropertyValueEditor;
            private readonly MediaPickerPropertyEditor.MediaPickerPropertyValueEditor _mediaPickerPropertyValueEditor;
            private readonly IImageUrlGenerator _imageUrlGenerator;
            private readonly IHtmlMacroParameterParser _macroParameterParser;

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
                IIOHelper ioHelper,
                IHtmlMacroParameterParser macroParameterParser)
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
                _macroParameterParser = macroParameterParser;
            }

            [Obsolete("Use the constructor which takes an IHtmlMacroParameterParser instead")]
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
                : this(
                    dataValueEditorFactory,
                    attribute,
                    backOfficeSecurityAccessor,
                    localizedTextService,
                    imageSourceParser,
                    pastedImages,
                    shortStringHelper,
                    imageUrlGenerator,
                    jsonSerializer,
                    ioHelper,
                    StaticServiceProvider.Instance.GetRequiredService<IHtmlMacroParameterParser>())
            {
            }

            /// <summary>
            /// Format the data for persistence
            /// This to ensure if a RTE is used in a Grid cell/control that we parse it for tmp stored images
            /// to persist to the media library when we go to persist this to the DB
            /// </summary>
            /// <param name="editorValue"></param>
            /// <param name="currentValue"></param>
            /// <returns></returns>
            public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
            {
                if (editorValue.Value == null)
                {
                    return null;
                }

                // editorValue.Value is a JSON string of the grid
                var rawJson = editorValue.Value.ToString();
                if (rawJson.IsNullOrWhiteSpace())
                {
                    return null;
                }

                var config = editorValue.DataTypeConfiguration as GridConfiguration;
                GuidUdi? mediaParent = config?.MediaParentId;
                Guid mediaParentId = mediaParent?.Guid ?? Guid.Empty;

                GridValue? grid = DeserializeGridValue(rawJson!, out var rtes, out _, out _);

                var userId = _backOfficeSecurityAccessor?.BackOfficeSecurity?.CurrentUser?.Id ?? Constants.Security.SuperUserId;

                if (rtes is null)
                {
                    return JsonConvert.SerializeObject(grid, Formatting.None);
                }
                // Process the rte values
                foreach (GridValue.GridControl rte in rtes)
                {
                    // Parse the HTML
                    var html = rte.Value?.ToString();

                    if (html is not null)
                    {
                        var parseAndSavedTempImages = _pastedImages.FindAndPersistPastedTempImages(html, mediaParentId, userId, _imageUrlGenerator);
                        var editorValueWithMediaUrlsRemoved = _imageSourceParser.RemoveImageSources(parseAndSavedTempImages);
                        rte.Value = editorValueWithMediaUrlsRemoved;
                    }
                }

                // Convert back to raw JSON for persisting
                return JsonConvert.SerializeObject(grid, Formatting.None);
            }

            /// <summary>
            /// Ensures that the rich text editor values are processed within the grid
            /// </summary>
            /// <param name="property"></param>
            /// <param name="culture"></param>
            /// <param name="segment"></param>
            /// <returns></returns>
            public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
            {
                var val = property.GetValue(culture, segment)?.ToString();
                if (val.IsNullOrWhiteSpace())
                {
                    return string.Empty;
                }

                GridValue? grid = DeserializeGridValue(val!, out var rtes, out _, out _);

                if (rtes is null)
                {
                    return null;
                }
                //process the rte values
                foreach (GridValue.GridControl rte in rtes.ToList())
                {
                    var html = rte.Value?.ToString();

                    if (html is not null)
                    {
                        var propertyValueWithMediaResolved = _imageSourceParser.EnsureImageSources(html);
                        rte.Value = propertyValueWithMediaResolved;
                    }

                }

                return grid;
            }

            private GridValue? DeserializeGridValue(string rawJson, out IEnumerable<GridValue.GridControl>? richTextValues, out IEnumerable<GridValue.GridControl>? mediaValues, out IEnumerable<GridValue.GridControl>? macroValues)
            {
                GridValue? grid = JsonConvert.DeserializeObject<GridValue>(rawJson);

                // Find all controls that use the RTE editor
                GridValue.GridControl[]? controls = grid?.Sections.SelectMany(x => x.Rows.SelectMany(r => r.Areas).SelectMany(a => a.Controls)).ToArray();
                richTextValues = controls?.Where(x => x.Editor.Alias.ToLowerInvariant() == "rte");
                mediaValues = controls?.Where(x => x.Editor.Alias.ToLowerInvariant() == "media");

                // Find all the macros
                macroValues = controls?.Where(x => x.Editor.Alias.ToLowerInvariant() == "macro");

                return grid;
            }

            /// <summary>
            /// Resolve references from <see cref="IDataValueEditor"/> values
            /// </summary>
            /// <param name="value"></param>
            /// <returns></returns>
            public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
            {
                var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();

                if (rawJson.IsNullOrWhiteSpace())
                {
                    yield break;
                }

                DeserializeGridValue(rawJson!, out IEnumerable<GridValue.GridControl>? richTextEditorValues, out IEnumerable<GridValue.GridControl>? mediaValues, out IEnumerable<GridValue.GridControl>? macroValues);

                if (richTextEditorValues is not null)
                {
                    foreach (UmbracoEntityReference umbracoEntityReference in richTextEditorValues.SelectMany(x =>
                                 _richTextPropertyValueEditor.GetReferences(x.Value)))
                    {
                        yield return umbracoEntityReference;
                    }
                }

                if (mediaValues is not null)
                {
                    foreach (UmbracoEntityReference umbracoEntityReference in mediaValues.Where(x => x.Value?.HasValues ?? false)
                                 .SelectMany(x => _mediaPickerPropertyValueEditor.GetReferences(x.Value!["udi"])))
                    {
                        yield return umbracoEntityReference;
                    }
                }

                if (macroValues is not null)
                {
                    foreach (UmbracoEntityReference umbracoEntityReference in _macroParameterParser.FindUmbracoEntityReferencesFromGridControlMacros(macroValues))
                    {
                        yield return umbracoEntityReference;
                    }
                }
            }
        }
    }
}
