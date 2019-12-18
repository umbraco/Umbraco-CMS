using System;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Tests.TestHelpers.Entities
{
    [Serializable]
    [DataContract(IsReference = true)]
    public class MockedEntity : EntityBase
    {
        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Value { get; set; }
    }

    public class CustomMockedEntity : MockedEntity
    {
        [DataMember]
        public string Title { get; set; }
    }
}
