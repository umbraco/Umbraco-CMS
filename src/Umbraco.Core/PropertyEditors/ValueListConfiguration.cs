using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents the ValueList editor configuration.
/// </summary>
public class ValueListConfiguration
{
    [ConfigurationField("items", "Configure", "multivalues", Description = "Add, remove or sort values for the list.")]
    public List<ValueListItem> Items { get; set; } = new();

    [DataContract]
    public class ValueListItem
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "value")]
        public string? Value { get; set; }
    }
}
