// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.TemplateQuery;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Integration.TestServerTest;
using Umbraco.Cms.Web.BackOffice.Controllers;
using Umbraco.Cms.Web.Common.Formatters;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Web.BackOffice.Controllers;

[TestFixture]
public class TemplateQueryControllerTests : UmbracoTestServerTestBase
{
    [Test]
    public async Task GetContentTypes__Ensure_camel_case()
    {
        var url = PrepareApiControllerUrl<TemplateQueryController>(x => x.GetContentTypes());

        // Act
        var response = await Client.GetAsync(url);

        var body = await response.Content.ReadAsStringAsync();

        body = body.TrimStart(AngularJsonMediaTypeFormatter.XsrfPrefix);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            Assert.DoesNotThrow(() => JsonConvert.DeserializeObject<ContentTypeModel[]>(body));

            var jtokens = JsonConvert.DeserializeObject<JToken[]>(body);
            foreach (var jToken in jtokens)
            {
                var alias = nameof(ContentTypeModel.Alias);
                var camelCaseAlias = alias.ToCamelCase();
                Assert.IsNotNull(jToken.Value<string>(camelCaseAlias), $"'{jToken}' do not contain the key '{camelCaseAlias}' in the expected casing");
                Assert.IsNull(jToken.Value<string>(alias), $"'{jToken}' do contain the key '{alias}', which was not expect in that casing");
            }
        });
    }
}
