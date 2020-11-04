using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the file upload address value editor.
    /// </summary>
    public class FileUploadConfiguration
    {

        [ConfigurationField("fileExtensions", "Accepted file extensions", "multivalues")]
        public List<ValueListItem> FileExtensions { get; set; } = new List<ValueListItem>();

        public class ValueListItem
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("value")]
            public string Value { get; set; }
        }
    }
}
