using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Serialization;

[TestFixture]
public class AutoInterningStringConverterTests
{
    [Test]
    public void Intern_Property_String()
    {
        var str1 = "Hello";
        var obj = new Test { Name = str1 + Guid.NewGuid() };

        // ensure the raw value is not interned
        Assert.IsNull(string.IsInterned(obj.Name));

        var serialized = JsonConvert.SerializeObject(obj);
        obj = JsonConvert.DeserializeObject<Test>(serialized);

        Assert.IsNotNull(string.IsInterned(obj.Name));
    }

    [Test]
    public void Intern_Property_Dictionary()
    {
        var str1 = "key";
        var obj = new Test
        {
            Values = new Dictionary<string, int> { [str1 + Guid.NewGuid()] = 0, [str1 + Guid.NewGuid()] = 1 },
        };

        // ensure the raw value is not interned
        Assert.IsNull(string.IsInterned(obj.Values.Keys.First()));
        Assert.IsNull(string.IsInterned(obj.Values.Keys.Last()));

        var serialized = JsonConvert.SerializeObject(obj);
        obj = JsonConvert.DeserializeObject<Test>(serialized);

        Assert.IsNotNull(string.IsInterned(obj.Values.Keys.First()));
        Assert.IsNotNull(string.IsInterned(obj.Values.Keys.Last()));
    }

    public class Test
    {
        [JsonConverter(typeof(AutoInterningStringConverter))]
        public string Name { get; set; }

        [JsonConverter(typeof(AutoInterningStringKeyCaseInsensitiveDictionaryConverter<int>))]
        public Dictionary<string, int> Values { get; set; } = new();
    }
}
