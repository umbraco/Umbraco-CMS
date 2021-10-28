using System.Collections.Generic;
using System.Runtime.Serialization;
using Umbraco.Core.Models.ContentEditing;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "contentType", Namespace = "")]
    public class DocumentTypeDisplay : ContentTypeCompositionDisplay<PropertyTypeDisplay>
    {
        public DocumentTypeDisplay()
        {
            //initialize collections so at least their never null
            AllowedTemplates = new List<EntityBasic>();
        }

        //name, alias, icon, thumb, desc, inherited from the content type

        // Templates
        [DataMember(Name = "allowedTemplates")]
        public IEnumerable<EntityBasic> AllowedTemplates { get; set; }

        [DataMember(Name = "defaultTemplate")]
        public EntityBasic DefaultTemplate { get; set; }

        [DataMember(Name = "allowCultureVariant")]
        public bool AllowCultureVariant { get; set; }

        [DataMember(Name = "allowSegmentVariant")]
        public bool AllowSegmentVariant { get; set; }

        [DataMember(Name = "apps")]
        public IEnumerable<ContentApp> ContentApps { get; set; }

        [DataMember(Name = "historyCleanup")]
        public HistoryCleanupViewModel HistoryCleanup { get; set; }
    }
}
