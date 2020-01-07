using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Templates;

namespace Umbraco.Web.PropertyEditors
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
        private IMediaService _mediaService;
        private IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
        private IUmbracoContextAccessor _umbracoContextAccessor;
        private ILogger _logger;

        public GridPropertyEditor(ILogger logger, IMediaService mediaService, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger)
        {
            _mediaService = mediaService;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
            _umbracoContextAccessor = umbracoContextAccessor;
            _logger = logger;
        }

        public override IPropertyIndexValueFactory PropertyIndexValueFactory => new GridPropertyIndexValueFactory();

        /// <summary>
        /// Overridden to ensure that the value is validated
        /// </summary>
        /// <returns></returns>
        protected override IDataValueEditor CreateValueEditor() => new GridPropertyValueEditor(Attribute, _mediaService, _contentTypeBaseServiceProvider, _umbracoContextAccessor, _logger);

        protected override IConfigurationEditor CreateConfigurationEditor() => new GridConfigurationEditor();

        internal class GridPropertyValueEditor : DataValueEditor
        {
            private IMediaService _mediaService;
            private IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
            private IUmbracoContextAccessor _umbracoContextAccessor;
            private ILogger _logger;

            public GridPropertyValueEditor(DataEditorAttribute attribute, IMediaService mediaService, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, IUmbracoContextAccessor umbracoContextAccessor, ILogger logger)
                : base(attribute)
            {
                _mediaService = mediaService;
                _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
                _umbracoContextAccessor = umbracoContextAccessor;
                _logger = logger;
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

                var grid = DeserializeGridValue(rawJson, out var rtes);

                var userId = _umbracoContextAccessor.UmbracoContext?.Security?.CurrentUser?.Id ?? Constants.Security.SuperUserId;

                // Process the rte values
                foreach (var rte in rtes)
                {
                    // Parse the HTML
                    var html = rte.Value?.ToString();

                    var parseAndSavedTempImages = TemplateUtilities.FindAndPersistPastedTempImages(html, mediaParentId, userId, _mediaService, _contentTypeBaseServiceProvider, _logger);
                    var editorValueWithMediaUrlsRemoved = TemplateUtilities.RemoveMediaUrlsFromTextString(parseAndSavedTempImages);

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
            public override object ToEditor(Property property, IDataTypeService dataTypeService, string culture = null, string segment = null)
            {
                var val = property.GetValue(culture, segment)?.ToString();
                if (val.IsNullOrWhiteSpace()) return string.Empty;

                var grid = DeserializeGridValue(val, out var rtes);

                //process the rte values
                foreach (var rte in rtes.ToList())
                {
                    var html = rte.Value?.ToString();

                    var propertyValueWithMediaResolved = TemplateUtilities.ResolveMediaFromTextString(html);
                    rte.Value = propertyValueWithMediaResolved;
                }

                return grid;
            }

            private GridValue DeserializeGridValue(string rawJson, out IEnumerable<GridValue.GridControl> richTextValues)
            {
                var grid = JsonConvert.DeserializeObject<GridValue>(rawJson);

                // Find all controls that use the RTE editor
                var controls = grid.Sections.SelectMany(x => x.Rows.SelectMany(r => r.Areas).SelectMany(a => a.Controls));
                richTextValues = controls.Where(x => x.Editor.Alias.ToLowerInvariant() == "rte");

                return grid;
            }
        }
    }
}
