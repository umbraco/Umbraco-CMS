using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// A model representing the configuration for a content type defined as a list container
    /// </summary>
    [DataContract(Name = "config", Namespace = "")]
    public class ContentTypeContainerConfiguration
    {
        /// <summary>
        /// The page size for the list
        /// </summary>
        [DataMember(Name = "pageSize")]
        public int PageSize { get; set; }

        /// <summary>
        /// The aliases additional columns displayed after the node name in the list
        /// </summary>
        [DataMember(Name = "additionalColumnAliases")]
        public string AdditionalColumnAliases { get; set; }
        
        /// <summary>
        /// The default order by column for the list
        /// </summary>
        [DataMember(Name = "orderBy")]
        public string OrderBy { get; set; }

        /// <summary>
        /// The default order direction for the list
        /// </summary>
        [DataMember(Name = "orderDirection")]
        public string OrderDirection { get; set; }

        /// <summary>
        /// Flag for whether bulk publishing is allowed
        /// </summary>
        [DataMember(Name = "allowBulkPublish")]
        public bool AllowBulkPublish { get; set; }

        /// <summary>
        /// Flag for whether bulk unpublishing is allowed
        /// </summary>
        [DataMember(Name = "allowBulkUnpublish")]
        public bool AllowBulkUnpublish { get; set; }

        /// <summary>
        /// Flag for whether bulk deletion is allowed
        /// </summary>
        [DataMember(Name = "allowBulkDelete")]
        public bool AllowBulkDelete { get; set; }

        /// <summary>
        /// The column details for the additional columns displayed after the node name in the list
        /// </summary>
        /// <remarks>This isn't persisted, but is calculated from the aliases when passed to the UI</remarks>
        [DataMember(Name = "additionalColumns")]
        public IList<AdditionalColumnDetail> AdditionalColumns { get; set; }

        [DataContract(Namespace = "")]
        public class AdditionalColumnDetail
        {
            [DataMember(Name = "alias")]
            public string Alias { get; set; }

            [DataMember(Name = "header")]
            public string Header { get; set; }

            [DataMember(Name = "localizationKey")]
            public string LocalizationKey { get; set; }

            [DataMember(Name = "allowSorting")]
            public bool AllowSorting { get; set; }
        }
    }
}
