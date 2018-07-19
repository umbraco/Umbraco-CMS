using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Validation;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "contentVariant", Namespace = "")]
    public class ContentVariantSave : IContentProperties<ContentPropertyBasic>
    {
        public ContentVariantSave()
        {
            Properties = new List<ContentPropertyBasic>();
        }

        [DataMember(Name = "name", IsRequired = true)]
        [RequiredForPersistence(AllowEmptyStrings = false, ErrorMessage = "Required")]
        public string Name { get; set; }

        [DataMember(Name = "properties")]
        public IEnumerable<ContentPropertyBasic> Properties { get; set; }

        /// <summary>
        /// The culture of this variant, if this is invariant than this is null or empty
        /// </summary>
        [DataMember(Name = "culture")]
        public string Culture { get; set; }

        /// <summary>
        /// Indicates if the variant should be published or unpublished
        /// </summary>
        [DataMember(Name = "publish")]
        public bool Publish { get; set; }

     
    }
}
