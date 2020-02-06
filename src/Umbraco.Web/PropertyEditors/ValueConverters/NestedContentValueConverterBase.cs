using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ValueConverters
{
    public abstract class NestedContentValueConverterBase : PropertyValueConverterBase
    {

        protected NestedContentValueConverterBase(BlockEditorConverter blockEditorConverter, IPublishedModelFactory publishedModelFactory)
        {
            BlockEditorConverter = blockEditorConverter;
            PublishedModelFactory = publishedModelFactory;
        }

        protected BlockEditorConverter BlockEditorConverter { get; }
        protected IPublishedModelFactory PublishedModelFactory { get; }

        public static bool IsNested(IPublishedPropertyType publishedProperty)
        {
            return publishedProperty.EditorAlias.InvariantEquals(Constants.PropertyEditors.Aliases.NestedContent);
        }

        public static bool IsNestedSingle(IPublishedPropertyType publishedProperty)
        {
            if (!IsNested(publishedProperty))
                return false;

            var config = publishedProperty.DataType.ConfigurationAs<NestedContentConfiguration>();
            return config.MinItems == 1 && config.MaxItems == 1;
        }

        public static bool IsNestedMany(IPublishedPropertyType publishedProperty)
        {
            return IsNested(publishedProperty) && !IsNestedSingle(publishedProperty);
        }

        
    }
}
