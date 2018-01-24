using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Used to dynamically change the tag group and storage type based on the pre-values
    /// </summary>
    internal class TagPropertyEditorTagDefinition : TagPropertyDefinition
    {
        public TagPropertyEditorTagDefinition(ContentPropertyData propertySaving, SupportTagsAttribute tagsAttribute)
            : base(propertySaving, tagsAttribute)
        { }

        public override string TagGroup
        {
            get
            {
                var config = ConfigurationEditor.ConfigurationAs<TagConfiguration>(PropertySaving.DataTypeConfiguration);
                return string.IsNullOrWhiteSpace(config.Group) ? "default" : config.Group;
            }
        }

        public override TagCacheStorageType StorageType
        {
            get
            {
                var config = ConfigurationEditor.ConfigurationAs<TagConfiguration>(PropertySaving.DataTypeConfiguration);
                return config.StorageType;
            }
        }
    }
}
