using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Web.Compose;

namespace Umbraco.Tests.PropertyEditors
{
    [TestFixture]
    public class NestedContentPropertyComponentTests
    {
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
            var result = component.CreateNestedContentKeys(json, false, guidFactory);

            Assert.AreEqual(JsonConvert.DeserializeObject(expected).ToString(), JsonConvert.DeserializeObject(result).ToString());
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
			}
		]
	}
]";

            var expected = json
                .Replace("04a6dba8-813c-4144-8aca-86a3f24ebf08", guids[0].ToString())
                .Replace("d8e214d8-c5a5-4b45-9b51-4050dd47f5fa", guids[1].ToString())
                .Replace("dccf550c-3a05-469e-95e1-a8f560f788c2", guids[2].ToString())
                .Replace("fbde4288-8382-4e13-8933-ed9c160de050", guids[3].ToString());

            var component = new NestedContentPropertyComponent();
            var result = component.CreateNestedContentKeys(json, false, guidFactory);

            Assert.AreEqual(JsonConvert.DeserializeObject(expected).ToString(), JsonConvert.DeserializeObject(result).ToString());
        }

        [Test]
        public void One_Level_Nesting_Escaped()
        {
            var guids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
            var guidCounter = 0;
            Func<Guid> guidFactory = () => guids[guidCounter++];

            // we need to ensure the escaped json is consistent with how it will be re-escaped after parsing
            // and this is how to do that, the result will also include quotes around it.
            var subJsonEscaped = JsonConvert.ToString(JsonConvert.DeserializeObject(@"[{
				""key"": ""dccf550c-3a05-469e-95e1-a8f560f788c2"",
				""name"": ""Item 1"",
				""ncContentTypeAlias"": ""text"",
				""text"": ""woot""
			}, {
				""key"": ""fbde4288-8382-4e13-8933-ed9c160de050"",
				""name"": ""Item 2"",
				""ncContentTypeAlias"": ""text"",
				""text"": ""zoot""
			}
		]").ToString());

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
	}
]";

            var expected = json
                .Replace("04a6dba8-813c-4144-8aca-86a3f24ebf08", guids[0].ToString())
                .Replace("d8e214d8-c5a5-4b45-9b51-4050dd47f5fa", guids[1].ToString())
                .Replace("dccf550c-3a05-469e-95e1-a8f560f788c2", guids[2].ToString())
                .Replace("fbde4288-8382-4e13-8933-ed9c160de050", guids[3].ToString());

            var component = new NestedContentPropertyComponent();
            var result = component.CreateNestedContentKeys(json, false, guidFactory);

            Assert.AreEqual(JsonConvert.DeserializeObject(expected).ToString(), JsonConvert.DeserializeObject(result).ToString());
        }

        // TODO: Write tests for:
        // * onlyMissingKeys = true for all combinations
        // * 3 levels of nesting including when NC -> unknown complex editor -> NC

    }
}
