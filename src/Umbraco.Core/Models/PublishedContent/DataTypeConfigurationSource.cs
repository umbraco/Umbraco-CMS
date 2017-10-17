using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Core.Models.PublishedContent
{
    /// <summary>
    /// Provides a default <see cref="IDataTypeConfigurationSource"/> implementation.
    /// </summary>
    internal class DataTypeConfigurationSource : IDataTypeConfigurationSource
    {
        private readonly IDataTypeService _dataTypeService;
        private readonly PropertyEditorCollection _propertyEditors;

        // fixme - we might want to have a way to load *all* configs in a fast way?

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTypeConfigurationSource"/> class.
        /// </summary>
        /// <param name="dataTypeService">A data type service.</param>
        /// <param name="propertyEditors">The collection of property editors.</param>
        public DataTypeConfigurationSource(IDataTypeService dataTypeService, PropertyEditorCollection propertyEditors)
        {
            _dataTypeService = dataTypeService;
            _propertyEditors = propertyEditors;
        }

        /// <inheritdoc />
        public object GetDataTypeConfiguration(string editorAlias, int id)
        {
            // fixme - we need a more efficient dataTypeService way of getting these
            // fixme - would be nice not to pass editorAlias but annoying for tests?
            //var datatype = _dataTypeService.GetDataTypeDefinitionById(id);
            var collection = _dataTypeService.GetPreValuesCollectionByDataTypeId(id);
            var editor = _propertyEditors[editorAlias /*datatype.PropertyEditorAlias*/];
            return editor.MapDataTypeConfiguration(collection);
        }
    }
}
