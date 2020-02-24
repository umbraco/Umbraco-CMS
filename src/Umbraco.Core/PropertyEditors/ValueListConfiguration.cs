﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Umbraco.Core.PropertyEditors
{
    /// <summary>
    /// Represents the ValueList editor configuration.
    /// </summary>
    public class ValueListConfiguration
    {
        [ConfigurationField("items", "Configure", "multivalues", Description = "Add, remove or sort values for the list.")]
        public List<ValueListItem> Items { get; set; } = new List<ValueListItem>();

        [DataContract]
        public class ValueListItem
        {
            [DataMember(Name = "id")]
            public int Id { get; set; }

            [DataMember(Name = "value")]
            public string Value { get; set; }
        }
    }
}
