// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Extensions;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Extensions;

[TestFixture]
public class DocumentUrlAliasServiceExtensionsTests
{
    private static readonly IDocumentUrlAliasService _service = Mock.Of<IDocumentUrlAliasService>();

    [TestCase("some-alias", "some-alias")]
    [TestCase("  some-alias  ", "some-alias")]
    [TestCase("/some-alias/", "some-alias")]
    [TestCase("//some-alias//", "some-alias")]
    [TestCase("Some-Mixed-Case", "some-mixed-case")]
    [TestCase("  /Some/Nested-Alias/  ", "some/nested-alias")]
    public void NormalizeAlias_Trims_Whitespace_And_Slashes_And_Lowercases(string alias, string expected)
        => Assert.AreEqual(expected, _service.NormalizeAlias(alias));
}
