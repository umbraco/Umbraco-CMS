using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Newtonsoft.Json;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Web.Templates;
using Umbraco.Web.Composing;
using Umbraco.Core.Services;
using System;

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

        public GridPropertyEditor(ILogger logger, IMediaService mediaService, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider)
            : base(logger)
        {
            _mediaService = mediaService;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
        }

        public override IPropertyIndexValueFactory PropertyIndexValueFactory => new GridPropertyIndexValueFactory();

        /// <summary>
        /// Overridden to ensure that the value is validated
        /// </summary>
        /// <returns></returns>
        protected override IDataValueEditor CreateValueEditor() => new GridPropertyValueEditor(Attribute, _mediaService, _contentTypeBaseServiceProvider);

        protected override IConfigurationEditor CreateConfigurationEditor() => new GridConfigurationEditor();

        internal class GridPropertyValueEditor : DataValueEditor
        {
            private IMediaService _mediaService;
            private IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;

            public GridPropertyValueEditor(DataEditorAttribute attribute, IMediaService mediaService, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider)
                : base(attribute)
            {
                _mediaService = mediaService;
                _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
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

                var config = editorValue.DataTypeConfiguration as RichTextConfiguration;
                var mediaParent = config?.MediaParentId;
                Guid mediaParentId;

                if (mediaParent == null)
                    mediaParentId = Guid.Empty;
                else
                    mediaParentId = mediaParent.Guid;

                // editorValue.Value is a JSON string of the grid
                var rawJson = editorValue.Value.ToString();
                var grid = JsonConvert.DeserializeObject<GridValue>(rawJson);

                // Find all controls that use the RTE editor
                var controls = grid.Sections.SelectMany(x => x.Rows.SelectMany(r => r.Areas).SelectMany(a => a.Controls));
                var rtes = controls.Where(x => x.Editor.Alias.ToLowerInvariant() == "rte");

                foreach(var rte in rtes)
                {
                    // Parse the HTML
                    var html = rte.Value?.ToString();
                    
                    var userId = Current.UmbracoContext.Security.CurrentUser.Id;

                    var parsedHtml = TemplateUtilities.FindAndPersistPastedTempImages(html, mediaParentId, userId, _mediaService, _contentTypeBaseServiceProvider);
                    rte.Value = parsedHtml;
                }

                // Convert back to raw JSON for persisting
                return JsonConvert.SerializeObject(grid);
            }
        }
    }
}
