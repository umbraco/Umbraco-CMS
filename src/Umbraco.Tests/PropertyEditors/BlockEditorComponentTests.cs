using Newtonsoft.Json;
using NUnit.Framework;
using System;
using Umbraco.Web.Compose;

namespace Umbraco.Tests.PropertyEditors
{
    [TestFixture]
    public class BlockEditorComponentTests
    {
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            
        };

        [Test]
        public void No_Nesting()
        {
            var guids = new[] { Guid.NewGuid(), Guid.NewGuid() };
            var guidCounter = 0;
            Func<Guid> guidFactory = () => guids[guidCounter++];

            var json = @"{
    ""layout"":
    {
        ""Umbraco.BlockList"": [
            {
                ""contentUdi"": ""umb://element/036ce82586a64dfba2d523a99ed80f58""
            },
            {
                ""contentUdi"": ""umb://element/48288c21a38a40ef82deb3eda90a58f6""
            }
        ]
    },
    ""contentData"": [
        {
            ""contentTypeKey"": ""d6ce4a86-91a2-45b3-a99c-8691fc1fb020"",
            ""udi"": ""umb://element/036ce82586a64dfba2d523a99ed80f58"",
            ""featureName"": ""Hello"",
            ""featureDetails"": ""World""
        },
        {
            ""contentTypeKey"": ""d6ce4a86-91a2-45b3-a99c-8691fc1fb020"",
            ""udi"": ""umb://element/48288c21a38a40ef82deb3eda90a58f6"",
            ""featureName"": ""Another"",
            ""featureDetails"": ""Feature""
        }
    ],
    ""settingsData"": []
}";


            var expected = json
                .Replace("036ce82586a64dfba2d523a99ed80f58", guids[0].ToString("N"))
                .Replace("48288c21a38a40ef82deb3eda90a58f6", guids[1].ToString("N"));

            var component = new BlockEditorComponent();
            var result = component.CreateNestedContentKeys(json, false, guidFactory);

            var expectedJson = JsonConvert.DeserializeObject(expected, _serializerSettings).ToString();
            var resultJson = JsonConvert.DeserializeObject(result, _serializerSettings).ToString();
            Console.WriteLine(expectedJson);
            Console.WriteLine(resultJson);
            Assert.AreEqual(expectedJson, resultJson);
        }

        [Test]
        public void One_Level_Nesting_Escaped()
        {
            var guids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var guidCounter = 0;
            Func<Guid> guidFactory = () => guids[guidCounter++];

            // we need to ensure the escaped json is consistent with how it will be re-escaped after parsing
            // and this is how to do that, the result will also include quotes around it.
            var innerJson = JsonConvert.DeserializeObject(@"{
    ""layout"":
    {
        ""Umbraco.BlockList"": [
            {
                ""contentUdi"": ""umb://element/4C44CE6B3A5C4F5F8F15E3DC24819A9E""
            },
            {
                ""contentUdi"": ""umb://element/A062C06D6B0B44AC892B35D90309C7F8""
            }
        ]
    },
    ""contentData"": [
        {
            ""contentTypeKey"": ""d6ce4a86-91a2-45b3-a99c-8691fc1fb020"",
            ""udi"": ""umb://element/4C44CE6B3A5C4F5F8F15E3DC24819A9E"",
            ""featureName"": ""Hello"",
            ""featureDetails"": ""World""
        },
        {
            ""contentTypeKey"": ""d6ce4a86-91a2-45b3-a99c-8691fc1fb020"",
            ""udi"": ""umb://element/A062C06D6B0B44AC892B35D90309C7F8"",
            ""featureName"": ""Another"",
            ""featureDetails"": ""Feature""
        }
    ],
    ""settingsData"": []
}", _serializerSettings);

            var serializedInnerJson = JsonConvert.SerializeObject(innerJson, _serializerSettings);

            var subJsonEscaped = JsonConvert.ToString(serializedInnerJson);

            var json = @"{
    ""layout"":
    {
        ""Umbraco.BlockList"": [
            {
                ""contentUdi"": ""umb://element/036ce82586a64dfba2d523a99ed80f58""
            },
            {
                ""contentUdi"": ""umb://element/48288c21a38a40ef82deb3eda90a58f6""
            }
        ]
    },
    ""contentData"": [
        {
            ""contentTypeKey"": ""d6ce4a86-91a2-45b3-a99c-8691fc1fb020"",
            ""udi"": ""umb://element/036ce82586a64dfba2d523a99ed80f58"",
            ""featureName"": ""Hello"",
            ""featureDetails"": ""World"",
            ""subFeatures"": " + subJsonEscaped + @"
        },
        {
            ""contentTypeKey"": ""d6ce4a86-91a2-45b3-a99c-8691fc1fb020"",
            ""udi"": ""umb://element/48288c21a38a40ef82deb3eda90a58f6"",
            ""featureName"": ""Another"",
            ""featureDetails"": ""Feature""
        }
    ],
    ""settingsData"": []
}";

            var expected = json
                .Replace("036ce82586a64dfba2d523a99ed80f58", guids[0].ToString("N"))
                .Replace("48288c21a38a40ef82deb3eda90a58f6", guids[1].ToString("N"))
                .Replace("4C44CE6B3A5C4F5F8F15E3DC24819A9E", guids[2].ToString("N"))
                .Replace("A062C06D6B0B44AC892B35D90309C7F8", guids[3].ToString("N"));

            var component = new BlockEditorComponent();
            var result = component.CreateNestedContentKeys(json, false, guidFactory);

            var expectedJson = JsonConvert.DeserializeObject(expected, _serializerSettings).ToString();
            var resultJson = JsonConvert.DeserializeObject(result, _serializerSettings).ToString();
            Console.WriteLine(expectedJson);
            Console.WriteLine(resultJson);
            Assert.AreEqual(expectedJson, resultJson);
        }

    }
}
