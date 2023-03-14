// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Linq;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Manifest;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Manifest;

[TestFixture]
public class ManifestContentAppTests
{
    [Test]
    public void Test()
    {
        var contentType = Mock.Of<IContentType>();
        Mock.Get(contentType).Setup(x => x.Alias).Returns("type1");
        var content = Mock.Of<IContent>();
        Mock.Get(content).Setup(x => x.ContentType).Returns(new SimpleContentType(contentType));

        var group1 = Mock.Of<IReadOnlyUserGroup>();
        Mock.Get(group1).Setup(x => x.Alias).Returns("group1");
        var group2 = Mock.Of<IReadOnlyUserGroup>();
        Mock.Get(group2).Setup(x => x.Alias).Returns("group2");

        // no rule = ok
        AssertDefinition(content, true, Array.Empty<string>(), new[] { group1, group2 });

        // wildcards = ok
        AssertDefinition(content, true, new[] { "+content/*" }, new[] { group1, group2 });
        AssertDefinition(content, false, new[] { "+media/*" }, new[] { group1, group2 });

        // explicitly enabling / disabling
        AssertDefinition(content, true, new[] { "+content/type1" }, new[] { group1, group2 });
        AssertDefinition(content, false, new[] { "-content/type1" }, new[] { group1, group2 });

        // when there are type rules, failing to approve the type = no app
        AssertDefinition(content, false, new[] { "+content/type2" }, new[] { group1, group2 });
        AssertDefinition(content, false, new[] { "+media/type1" }, new[] { group1, group2 });

        // can have multiple rule, first one that matches = end
        AssertDefinition(content, false, new[] { "-content/type1", "+content/*" }, new[] { group1, group2 });
        AssertDefinition(content, true, new[] { "-content/type2", "+content/*" }, new[] { group1, group2 });
        AssertDefinition(content, true, new[] { "+content/*", "-content/type1" }, new[] { group1, group2 });

        // when there are role rules, failing to approve a role = no app
        AssertDefinition(content, false, new[] { "+role/group33" }, new[] { group1, group2 });

        // wildcards = ok
        AssertDefinition(content, true, new[] { "+role/*" }, new[] { group1, group2 });

        // explicitly enabling / disabling
        AssertDefinition(content, true, new[] { "+role/group1" }, new[] { group1, group2 });
        AssertDefinition(content, false, new[] { "-role/group1" }, new[] { group1, group2 });

        // can have multiple rule, first one that matches = end
        AssertDefinition(content, true, new[] { "+role/group1", "-role/group2" }, new[] { group1, group2 });

        // mixed type and role rules, both are evaluated and need to match
        AssertDefinition(content, true, new[] { "+role/group1", "+content/type1" }, new[] { group1, group2 });
        AssertDefinition(content, false, new[] { "+role/group1", "+content/type2" }, new[] { group1, group2 });
        AssertDefinition(content, false, new[] { "+role/group33", "+content/type1" }, new[] { group1, group2 });
    }

    private void AssertDefinition(object source, bool expected, string[] show, IReadOnlyUserGroup[] groups)
    {
        var definition = JsonConvert.DeserializeObject<ManifestContentAppDefinition>("{" +
            (show.Length == 0
                ? string.Empty
                : " \"show\": [" + string.Join(",", show.Select(x => "\"" + x + "\"")) + "] ") + "}");
        var factory = new ManifestContentAppFactory(definition, TestHelper.IOHelper);
        var app = factory.GetContentAppFor(source, groups);
        if (expected)
        {
            Assert.IsNotNull(app);
        }
        else
        {
            Assert.IsNull(app);
        }
    }
}
