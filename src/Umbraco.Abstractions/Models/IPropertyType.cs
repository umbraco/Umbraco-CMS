using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    public interface IPropertyType : IEntity
    {
        [DataMember]
        string Name { get; }
        [DataMember]
        string Alias { get; }
        [DataMember]
        string Description { get; }
        [DataMember]
        int DataTypeId { get; }
        [DataMember]
        Guid DataTypeKey { get; }
        [DataMember]
        string PropertyEditorAlias { get; }
        [DataMember]
        ValueStorageType ValueStorageType { get; }
        [DataMember]
        Lazy<int> PropertyGroupId { get; }
        [DataMember]
        bool Mandatory { get; }
        [DataMember]
        int SortOrder { get; }
        [DataMember]
        string ValidationRegExp { get; }

        bool SupportsPublishing { get;  }

        ContentVariation Variations { get; }



        bool SupportsVariation(string culture, string segment, bool wildcards = false);
        object ConvertAssignedValue(object value);


    }
}
