// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Web.Common.AngularIntegration;

[TestFixture]
public class ContentModelSerializationTests
{
    [Test]
    public void Content_Display_To_Json()
    {
        // create 3 tabs with 3 properties each
        var tabs = new List<Tab<ContentPropertyDisplay>>();
        for (var tabIndex = 0; tabIndex < 3; tabIndex++)
        {
            var props = new List<ContentPropertyDisplay>();
            for (var propertyIndex = 0; propertyIndex < 3; propertyIndex++)
            {
                props.Add(new ContentPropertyDisplay
                {
                    Alias = "property" + propertyIndex,
                    Label = "Property " + propertyIndex,
                    Id = propertyIndex,
                    Value = "value" + propertyIndex,
                    Config = new Dictionary<string, object> { { propertyIndex.ToInvariantString(), "value" } },
                    Description = "Description " + propertyIndex,
                    View = "~/Views/View" + propertyIndex,
                    HideLabel = false,
                });
            }

            tabs.Add(new Tab<ContentPropertyDisplay>
            {
                Alias = "Tab" + tabIndex,
                Label = "Tab" + tabIndex,
                Properties = props,
            });
        }

        var displayModel = new ContentItemDisplay
        {
            Id = 1234,
            Variants = new List<ContentVariantDisplay> { new() { Name = "Test", Tabs = tabs } },
        };

        var json = JsonConvert.SerializeObject(displayModel);

        var jObject = JObject.Parse(json);

        Assert.AreEqual("1234", jObject["id"].ToString());
        Assert.AreEqual("Test", jObject["variants"][0]["name"].ToString());

        var jsonTabs = jObject["variants"][0]["tabs"];
        Assert.AreEqual(3, jsonTabs.Count());
        for (var tab = 0; tab < jsonTabs.Count(); tab++)
        {
            Assert.AreEqual("Tab" + tab, jsonTabs[tab]["alias"].ToString());
            Assert.AreEqual("Tab" + tab, jsonTabs[tab]["label"].ToString());
            Assert.AreEqual(3, jsonTabs[tab]["properties"].Count());
            for (var prop = 0; prop < jsonTabs[tab]["properties"].Count(); prop++)
            {
                Assert.AreEqual("property" + prop, jsonTabs[tab]["properties"][prop]["alias"].ToString());
                Assert.AreEqual("Property " + prop, jsonTabs[tab]["properties"][prop]["label"].ToString());
                Assert.AreEqual(prop, jsonTabs[tab]["properties"][prop]["id"].Value<int>());
                Assert.AreEqual("value" + prop, jsonTabs[tab]["properties"][prop]["value"].ToString());
                Assert.AreEqual("{\"" + prop + "\":\"value\"}", jsonTabs[tab]["properties"][prop]["config"].ToString(Formatting.None));
                Assert.AreEqual("Description " + prop, jsonTabs[tab]["properties"][prop]["description"].ToString());
                Assert.AreEqual(false, jsonTabs[tab]["properties"][prop]["hideLabel"].Value<bool>());
            }
        }
    }
}
