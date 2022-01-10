using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Web.Compose;

namespace Umbraco.Tests.PropertyEditors
{

    [TestFixture]
    public class NestedContentPropertyComponentTests
    {
        private static void AreEqualJson(string expected, string actual)
        {
            Assert.AreEqual(JToken.Parse(expected), JToken.Parse(actual));
        }

        [Test]
        public void Invalid_Json()
        {
            var component = new NestedContentPropertyComponent();

            Assert.DoesNotThrow(() => component.CreateNestedContentKeys("this is not json", true));
        }

        [Test]
        public void No_Nesting()
        {
            var guids = new[] { Guid.NewGuid(), Guid.NewGuid() };
            var guidCounter = 0;
            Func<Guid> guidFactory = () => guids[guidCounter++];

            var json = @"[
              {""key"":""04a6dba8-813c-4144-8aca-86a3f24ebf08"",""name"":""Item 1"",""ncContentTypeAlias"":""nested"",""text"":""woot""},
              {""key"":""d8e214d8-c5a5-4b45-9b51-4050dd47f5fa"",""name"":""Item 2"",""ncContentTypeAlias"":""nested"",""text"":""zoot""}
            ]";

            var expected = json
                .Replace("04a6dba8-813c-4144-8aca-86a3f24ebf08", guids[0].ToString())
                .Replace("d8e214d8-c5a5-4b45-9b51-4050dd47f5fa", guids[1].ToString());

            var component = new NestedContentPropertyComponent();
            var actual = component.CreateNestedContentKeys(json, false, guidFactory);

            AreEqualJson(expected, actual);
        }

        [Test]
        public void One_Level_Nesting_Unescaped()
        {
            var guids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var guidCounter = 0;
            Func<Guid> guidFactory = () => guids[guidCounter++];

            var json = @"[{
		        ""key"": ""04a6dba8-813c-4144-8aca-86a3f24ebf08"",
		        ""name"": ""Item 1"",
		        ""ncContentTypeAlias"": ""text"",
		        ""text"": ""woot""
	        }, {
		        ""key"": ""d8e214d8-c5a5-4b45-9b51-4050dd47f5fa"",
		        ""name"": ""Item 2"",
		        ""ncContentTypeAlias"": ""list"",
		        ""text"": ""zoot"",
		        ""subItems"": [{
				    ""key"": ""dccf550c-3a05-469e-95e1-a8f560f788c2"",
				    ""name"": ""Item 1"",
				    ""ncContentTypeAlias"": ""text"",
				    ""text"": ""woot""
			    }, {
				    ""key"": ""fbde4288-8382-4e13-8933-ed9c160de050"",
				    ""name"": ""Item 2"",
				    ""ncContentTypeAlias"": ""text"",
				    ""text"": ""zoot""
			    }]
	        }]";

            var expected = json
                .Replace("04a6dba8-813c-4144-8aca-86a3f24ebf08", guids[0].ToString())
                .Replace("d8e214d8-c5a5-4b45-9b51-4050dd47f5fa", guids[1].ToString())
                .Replace("dccf550c-3a05-469e-95e1-a8f560f788c2", guids[2].ToString())
                .Replace("fbde4288-8382-4e13-8933-ed9c160de050", guids[3].ToString());

            var component = new NestedContentPropertyComponent();
            var actual = component.CreateNestedContentKeys(json, false, guidFactory);

            AreEqualJson(expected, actual);
        }

        [Test]
        public void One_Level_Nesting_Escaped()
        {
            var guids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var guidCounter = 0;
            Func<Guid> guidFactory = () => guids[guidCounter++];

            // we need to ensure the escaped json is consistent with how it will be re-escaped after parsing
            // and this is how to do that, the result will also include quotes around it.
            var subJsonEscaped = JsonConvert.ToString(JToken.Parse(@"
            [{
				""key"": ""dccf550c-3a05-469e-95e1-a8f560f788c2"",
				""name"": ""Item 1"",
				""ncContentTypeAlias"": ""text"",
				""text"": ""woot""
			}, {
				""key"": ""fbde4288-8382-4e13-8933-ed9c160de050"",
				""name"": ""Item 2"",
				""ncContentTypeAlias"": ""text"",
				""text"": ""zoot""
			}]").ToString(Formatting.None));

            var json = @"[{
		        ""key"": ""04a6dba8-813c-4144-8aca-86a3f24ebf08"",
		        ""name"": ""Item 1"",
		        ""ncContentTypeAlias"": ""text"",
		        ""text"": ""woot""
	        }, {
		        ""key"": ""d8e214d8-c5a5-4b45-9b51-4050dd47f5fa"",
		        ""name"": ""Item 2"",
		        ""ncContentTypeAlias"": ""list"",
		        ""text"": ""zoot"",
		        ""subItems"":" + subJsonEscaped + @"
	        }]";

            var expected = json
                .Replace("04a6dba8-813c-4144-8aca-86a3f24ebf08", guids[0].ToString())
                .Replace("d8e214d8-c5a5-4b45-9b51-4050dd47f5fa", guids[1].ToString())
                .Replace("dccf550c-3a05-469e-95e1-a8f560f788c2", guids[2].ToString())
                .Replace("fbde4288-8382-4e13-8933-ed9c160de050", guids[3].ToString());

            var component = new NestedContentPropertyComponent();
            var actual = component.CreateNestedContentKeys(json, false, guidFactory);

            AreEqualJson(expected, actual);
        }

        [Test]
        public void Nested_In_Complex_Editor_Escaped()
        {
            var guids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var guidCounter = 0;
            Func<Guid> guidFactory = () => guids[guidCounter++];

            // we need to ensure the escaped json is consistent with how it will be re-escaped after parsing
            // and this is how to do that, the result will also include quotes around it.
            var subJsonEscaped = JsonConvert.ToString(JToken.Parse(@"[{
				""key"": ""dccf550c-3a05-469e-95e1-a8f560f788c2"",
				""name"": ""Item 1"",
				""ncContentTypeAlias"": ""text"",
				""text"": ""woot""
			}, {
				""key"": ""fbde4288-8382-4e13-8933-ed9c160de050"",
				""name"": ""Item 2"",
				""ncContentTypeAlias"": ""text"",
				""text"": ""zoot""
			}]").ToString(Formatting.None));

            // Complex editor such as the grid
            var complexEditorJsonEscaped = @"{
                ""name"": ""1 column layout"",
                ""sections"": [
                {
                    ""grid"": ""12"",
                    ""rows"": [
                    {
                        ""name"": ""Article"",
                        ""id"": ""b4f6f651-0de3-ef46-e66a-464f4aaa9c57"",
                        ""areas"": [
                        {
                            ""grid"": ""4"",
                            ""controls"": [{
                                ""value"": ""I am quote"",
                                ""editor"": {
                                    ""alias"": ""quote"",
                                    ""view"": ""textstring""
                                },
                                ""styles"": null,
                                ""config"": null
                            }],
                            ""styles"": null,
                            ""config"": null
                        },
                        {
                            ""grid"": ""8"",
                            ""controls"": [{
                                ""value"": ""Header"",
                                ""editor"": {
                                    ""alias"": ""headline"",
                                    ""view"": ""textstring""
                                },
                                ""styles"": null,
                                ""config"": null
                            },
                            {
                                ""value"": " + subJsonEscaped + @",
                                ""editor"": {
                                    ""alias"": ""madeUpNestedContent"",
                                    ""view"": ""madeUpNestedContentInGrid""
                                },
                                ""styles"": null,
                                ""config"": null
                            }],
                            ""styles"": null,
                            ""config"": null
                        }],
                        ""styles"": null,
                        ""config"": null
                    }]
                }]
            }";

            var json = @"[{
		        ""key"": ""04a6dba8-813c-4144-8aca-86a3f24ebf08"",
		        ""name"": ""Item 1"",
		        ""ncContentTypeAlias"": ""text"",
		        ""text"": ""woot""
	        }, {
		        ""key"": ""d8e214d8-c5a5-4b45-9b51-4050dd47f5fa"",
		        ""name"": ""Item 2"",
		        ""ncContentTypeAlias"": ""list"",
		        ""text"": ""zoot"",
		        ""subItems"":" + complexEditorJsonEscaped + @"
	        }]";

            var expected = json
                .Replace("04a6dba8-813c-4144-8aca-86a3f24ebf08", guids[0].ToString())
                .Replace("d8e214d8-c5a5-4b45-9b51-4050dd47f5fa", guids[1].ToString())
                .Replace("dccf550c-3a05-469e-95e1-a8f560f788c2", guids[2].ToString())
                .Replace("fbde4288-8382-4e13-8933-ed9c160de050", guids[3].ToString());

            var component = new NestedContentPropertyComponent();
            var actual = component.CreateNestedContentKeys(json, false, guidFactory);

            AreEqualJson(expected, actual);
        }

        [Test]
        public void No_Nesting_Generates_Keys_For_Missing_Items()
        {
            var guids = new[] { Guid.NewGuid() };
            var guidCounter = 0;
            Func<Guid> guidFactory = () => guids[guidCounter++];

            var json = @"[
              {""key"":""04a6dba8-813c-4144-8aca-86a3f24ebf08"",""name"":""Item 1 my key wont change"",""ncContentTypeAlias"":""nested"",""text"":""woot""},
              {""name"":""Item 2 was copied and has no key prop"",""ncContentTypeAlias"":""nested"",""text"":""zoot""}
            ]";

            var component = new NestedContentPropertyComponent();
            var result = component.CreateNestedContentKeys(json, true, guidFactory);

            // Ensure the new GUID is put in a key into the JSON
            Assert.IsTrue(result.Contains(guids[0].ToString()));

            // Ensure that the original key is NOT changed/modified & still exists
            Assert.IsTrue(result.Contains("04a6dba8-813c-4144-8aca-86a3f24ebf08"));
        }

        [Test]
        public void One_Level_Nesting_Escaped_Generates_Keys_For_Missing_Items()
        {
            var guids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var guidCounter = 0;
            Func<Guid> guidFactory = () => guids[guidCounter++];

            // we need to ensure the escaped json is consistent with how it will be re-escaped after parsing
            // and this is how to do that, the result will also include quotes around it.
            var subJsonEscaped = JsonConvert.ToString(JToken.Parse(@"[{
				""name"": ""Item 1"",
				""ncContentTypeAlias"": ""text"",
				""text"": ""woot""
			}, {
				""name"": ""Nested Item 2 was copied and has no key"",
				""ncContentTypeAlias"": ""text"",
				""text"": ""zoot""
			}]").ToString(Formatting.None));

            var json = @"[{
		        ""name"": ""Item 1 was copied and has no key"",
		        ""ncContentTypeAlias"": ""text"",
		        ""text"": ""woot""
	        }, {
		        ""key"": ""d8e214d8-c5a5-4b45-9b51-4050dd47f5fa"",
		        ""name"": ""Item 2"",
		        ""ncContentTypeAlias"": ""list"",
		        ""text"": ""zoot"",
		        ""subItems"":" + subJsonEscaped + @"
	        }]";

            var component = new NestedContentPropertyComponent();
            var result = component.CreateNestedContentKeys(json, true, guidFactory);

            // Ensure the new GUID is put in a key into the JSON for each item
            Assert.IsTrue(result.Contains(guids[0].ToString()));
            Assert.IsTrue(result.Contains(guids[1].ToString()));
            Assert.IsTrue(result.Contains(guids[2].ToString()));
        }

        [Test]
        public void Nested_In_Complex_Editor_Escaped_Generates_Keys_For_Missing_Items()
        {
            var guids = new[] { Guid.NewGuid(), Guid.NewGuid() };
            var guidCounter = 0;
            Func<Guid> guidFactory = () => guids[guidCounter++];

            // we need to ensure the escaped json is consistent with how it will be re-escaped after parsing
            // and this is how to do that, the result will also include quotes around it.
            var subJsonEscaped = JsonConvert.ToString(JToken.Parse(@"[{
				""key"": ""dccf550c-3a05-469e-95e1-a8f560f788c2"",
				""name"": ""Item 1"",
				""ncContentTypeAlias"": ""text"",
				""text"": ""woot""
			}, {
				""name"": ""Nested Item 2 was copied and has no key"",
				""ncContentTypeAlias"": ""text"",
				""text"": ""zoot""
			}]").ToString(Formatting.None));

            // Complex editor such as the grid
            var complexEditorJsonEscaped = @"{
                ""name"": ""1 column layout"",
                ""sections"": [
                {
                    ""grid"": ""12"",
                    ""rows"": [
                    {
                        ""name"": ""Article"",
                        ""id"": ""b4f6f651-0de3-ef46-e66a-464f4aaa9c57"",
                        ""areas"": [
                        {
                            ""grid"": ""4"",
                            ""controls"": [{
                                ""value"": ""I am quote"",
                                ""editor"": {
                                    ""alias"": ""quote"",
                                    ""view"": ""textstring""
                                },
                                ""styles"": null,
                                ""config"": null
                            }],
                            ""styles"": null,
                            ""config"": null
                        },
                        {
                            ""grid"": ""8"",
                            ""controls"": [{
                                ""value"": ""Header"",
                                ""editor"": {
                                    ""alias"": ""headline"",
                                    ""view"": ""textstring""
                                },
                                ""styles"": null,
                                ""config"": null
                            },
                            {
                                ""value"": " + subJsonEscaped + @",
                                ""editor"": {
                                    ""alias"": ""madeUpNestedContent"",
                                    ""view"": ""madeUpNestedContentInGrid""
                                },
                                ""styles"": null,
                                ""config"": null
                            }],
                            ""styles"": null,
                            ""config"": null
                        }],
                        ""styles"": null,
                        ""config"": null
                    }]
                }]
            }";

            var json = @"[{
		        ""key"": ""04a6dba8-813c-4144-8aca-86a3f24ebf08"",
		        ""name"": ""Item 1"",
		        ""ncContentTypeAlias"": ""text"",
		        ""text"": ""woot""
	        }, {
		        ""name"": ""Item 2 was copied and has no key"",
		        ""ncContentTypeAlias"": ""list"",
		        ""text"": ""zoot"",
		        ""subItems"":" + complexEditorJsonEscaped + @"
	        }]";

            var component = new NestedContentPropertyComponent();
            var result = component.CreateNestedContentKeys(json, true, guidFactory);

            // Ensure the new GUID is put in a key into the JSON for each item
            Assert.IsTrue(result.Contains(guids[0].ToString()));
            Assert.IsTrue(result.Contains(guids[1].ToString()));
        }
    }
}
