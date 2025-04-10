using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Api.Common.Serialization;
using Umbraco.Cms.Api.Management.Serialization;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Serialization;

[TestFixture]
public class SerializationTests
{
    [Test]
    public void Will_Serialize_To_Camel_Case()
    {
        var options = CreateJsonOptions();

        var objectToSerialize = new Foo();
        var json = JsonSerializer.Serialize(objectToSerialize, options.JsonSerializerOptions);

        Assert.AreEqual("{\"bar\":\"baz\"}", json);
    }

    [Test]
    public async Task Can_Serialize_To_Max_Depth()
    {
        var options = CreateJsonOptions();

        #region Object nested to 32+ levels

        var objectToSerialize = new Foo2
        {
            Level = 1,
            Foo = new Foo2
            {
                Level = 2,
                Foo = new Foo2
                {
                    Level = 3,
                    Foo = new Foo2
                    {
                        Level = 4,
                        Foo = new Foo2
                        {
                            Level = 5,
                            Foo = new Foo2
                            {
                                Level = 6,
                                Foo = new Foo2
                                {
                                    Level = 7,
                                    Foo = new Foo2
                                    {
                                        Level = 8,
                                        Foo = new Foo2
                                        {
                                            Level = 9,
                                            Foo = new Foo2
                                            {
                                                Level = 10,
                                                Foo = new Foo2
                                                {
                                                    Level = 11,
                                                    Foo = new Foo2
                                                    {
                                                        Level = 12,
                                                        Foo = new Foo2
                                                        {
                                                            Level = 13,
                                                            Foo = new Foo2
                                                            {
                                                                Level = 14,
                                                                Foo = new Foo2
                                                                {
                                                                    Level = 15,
                                                                    Foo = new Foo2
                                                                    {
                                                                        Level = 16,
                                                                        Foo = new Foo2
                                                                        {
                                                                            Level = 17,
                                                                            Foo = new Foo2
                                                                            {
                                                                                Level = 18,
                                                                                Foo = new Foo2
                                                                                {
                                                                                    Level = 19,
                                                                                    Foo = new Foo2
                                                                                    {
                                                                                        Level = 20,
                                                                                        Foo = new Foo2
                                                                                        {
                                                                                            Level = 21,
                                                                                            Foo = new Foo2
                                                                                            {
                                                                                                Level = 22,
                                                                                                Foo = new Foo2
                                                                                                {
                                                                                                    Level = 23,
                                                                                                    Foo = new Foo2
                                                                                                    {
                                                                                                        Level = 24,
                                                                                                        Foo = new Foo2
                                                                                                        {
                                                                                                            Level = 25,
                                                                                                            Foo = new Foo2
                                                                                                            {
                                                                                                                Level = 26,
                                                                                                                Foo = new Foo2
                                                                                                                {
                                                                                                                    Level = 27,
                                                                                                                    Foo = new Foo2
                                                                                                                    {
                                                                                                                        Level = 28,
                                                                                                                        Foo = new Foo2
                                                                                                                        {
                                                                                                                            Level = 29,
                                                                                                                            Foo = new Foo2
                                                                                                                            {
                                                                                                                                Level = 30,
                                                                                                                                Foo = new Foo2
                                                                                                                                {
                                                                                                                                    Level = 31,
                                                                                                                                    Foo = new Foo2
                                                                                                                                    {
                                                                                                                                        Level = 32,
                                                                                                                                        Foo = new Foo2
                                                                                                                                        {
                                                                                                                                            Level = 33
                                                                                                                                        },
                                                                                                                                    },
                                                                                                                                },
                                                                                                                            },
                                                                                                                        },
                                                                                                                    },
                                                                                                                },
                                                                                                            },
                                                                                                        },
                                                                                                    },
                                                                                                },
                                                                                            },
                                                                                        },
                                                                                    },
                                                                                },
                                                                            },
                                                                        },
                                                                    },
                                                                },
                                                            },
                                                        },
                                                    },
                                                },
                                            },
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
            },
        };

        #endregion

        var json = JsonSerializer.Serialize(objectToSerialize, options.JsonSerializerOptions);

        Assert.IsNotEmpty(json);
    }

    private static JsonOptions CreateJsonOptions()
    {
        var typeInfoResolver = new UmbracoJsonTypeInfoResolver(TestHelper.GetTypeFinder());
        var configurationOptions = new ConfigureUmbracoBackofficeJsonOptions(typeInfoResolver);
        var options = new JsonOptions();
        configurationOptions.Configure(global::Umbraco.Cms.Core.Constants.JsonOptionsNames.BackOffice, options);
        return options;
    }

    public class Foo
    {
        public string Bar { get; set; } = "baz";
    }

    public class Foo2
    {
        public int Level { get; set; }

        public Foo2? Foo { get; set; }
    }
}
