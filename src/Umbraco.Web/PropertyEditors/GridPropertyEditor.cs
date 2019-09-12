using Newtonsoft.Json;
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

        public GridPropertyEditor(ILogger logger, IMediaService mediaService, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, IUmbracoContextAccessor umbracoContextAccessor)
            : base(logger)
        {
            _mediaService = mediaService;
            _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public override IPropertyIndexValueFactory PropertyIndexValueFactory => new GridPropertyIndexValueFactory();

        /// <summary>
        /// Overridden to ensure that the value is validated
        /// </summary>
        /// <returns></returns>
        protected override IDataValueEditor CreateValueEditor() => new GridPropertyValueEditor(Attribute, _mediaService, _contentTypeBaseServiceProvider, _umbracoContextAccessor);

        protected override IConfigurationEditor CreateConfigurationEditor() => new GridConfigurationEditor();

        internal class GridPropertyValueEditor : DataValueEditor
        {
            private IMediaService _mediaService;
            private IContentTypeBaseServiceProvider _contentTypeBaseServiceProvider;
            private IUmbracoContextAccessor _umbracoContextAccessor;

            public GridPropertyValueEditor(DataEditorAttribute attribute, IMediaService mediaService, IContentTypeBaseServiceProvider contentTypeBaseServiceProvider, IUmbracoContextAccessor umbracoContextAccessor)
                : base(attribute)
            {
                _mediaService = mediaService;
                _contentTypeBaseServiceProvider = contentTypeBaseServiceProvider;
                _umbracoContextAccessor = umbracoContextAccessor;
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
                var grid = JsonConvert.DeserializeObject<GridValue>(rawJson);

                // Find all controls that use the RTE editor
                var controls = grid.Sections.SelectMany(x => x.Rows.SelectMany(r => r.Areas).SelectMany(a => a.Controls));
                var rtes = controls.Where(x => x.Editor.Alias.ToLowerInvariant() == "rte");

                foreach(var rte in rtes)
                {
                    // Parse the HTML
                    var html = rte.Value?.ToString();

                    var userId = _umbracoContextAccessor.UmbracoContext?.Security.CurrentUser.Id ?? -1;

                    // TODO: In future task(get the parent folder from this config) to save the media into
                    var parsedHtml = TemplateUtilities.FindAndPersistPastedTempImages(html, Constants.System.Root, userId, _mediaService, _contentTypeBaseServiceProvider);
                    rte.Value = parsedHtml;
                }

                // Convert back to raw JSON for persisting
                return JsonConvert.SerializeObject(grid);
            }
        }
    }
}
