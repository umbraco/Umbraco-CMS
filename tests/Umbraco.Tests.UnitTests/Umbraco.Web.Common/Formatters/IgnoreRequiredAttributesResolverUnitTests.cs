// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Runtime.Serialization;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Web.Common.Formatters;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.Formatters;

[TestFixture]
public class IgnoreRequiredAttributesResolverUnitTests
{
    [Test]
    public void Test()
    {
        const string emptyJsonObject = "{}";

        Assert.Multiple(() =>
        {
            // Ensure the deserialization throws if using default settings
            Assert.Throws<JsonSerializationException>(() =>
                JsonConvert.DeserializeObject<ObjectWithRequiredProperty>(emptyJsonObject));

            var actual = JsonConvert.DeserializeObject<ObjectWithRequiredProperty>(
                emptyJsonObject,
                new JsonSerializerSettings { ContractResolver = new IgnoreRequiredAttributesResolver() });

            Assert.IsNotNull(actual);
            Assert.IsNull(actual.Property);
        });
    }

    [DataContract(Name = "objectWithRequiredProperty", Namespace = "")]
    private class ObjectWithRequiredProperty
    {
        [DataMember(Name = "property", IsRequired = true)]
        public string Property { get; set; }
    }
}
