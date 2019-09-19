using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Models.Validation;
using Umbraco.Core.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    [DataContract(Name = "entity", Namespace = "")]
    public class EntityBasic
    {
        public EntityBasic()
        {
            AdditionalData = new Dictionary<string, object>();
        }

        [DataMember(Name = "name", IsRequired = true)]
        [RequiredForPersistence(AllowEmptyStrings = false, ErrorMessage = "Required")]
        public string Name { get; set; }

        [DataMember(Name = "id", IsRequired = true)]
        [Required]
        public object Id { get; set; }

        [DataMember(Name = "udi")]
        [ReadOnly(true)]
        [JsonConverter(typeof(UdiJsonConverter))]
        public Udi Udi { get; set; }

        [DataMember(Name = "icon")]
        public string Icon { get; set; }

        [DataMember(Name = "trashed")]
        [ReadOnly(true)]
        public bool Trashed { get; set; }

        /// <summary>
        /// This is the unique Id stored in the database - but could also be the unique id for a custom membership provider
        /// </summary>
        [DataMember(Name = "key")]
        public Guid Key { get; set; }

        [DataMember(Name = "parentId", IsRequired = true)]
        [Required]
        public int ParentId { get; set; }

        /// <summary>
        /// This will only be populated for some entities like macros
        /// </summary>
        /// <remarks>
        /// This is overrideable to specify different validation attributes if required
        /// </remarks>
        [DataMember(Name = "alias")]
        public virtual string Alias { get; set; }

        /// <summary>
        /// The path of the entity
        /// </summary>
        [DataMember(Name = "path")]
        public string Path { get; set; }
        
        /// <summary>
        /// A collection of extra data that is available for this specific entity/entity type
        /// </summary>
        [DataMember(Name = "metaData")]
        [ReadOnly(true)]
        public IDictionary<string, object> AdditionalData { get; private set; } 
    }
}
