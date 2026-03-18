// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Repositories;

[TestFixture]
internal sealed class DocumentRepositoryTests
{
    private IShortStringHelper _shortStringHelper = null!;

    [SetUp]
    public void SetUp()
    {
        // Simulate DefaultShortStringHelper behavior: lowercase, strip non-alphanumeric,
        // replace spaces with hyphens, collapse dashes.
        var mock = new Mock<IShortStringHelper>();
        mock.Setup(x => x.CleanStringForUrlSegment(It.IsAny<string>(), It.IsAny<string?>()))
            .Returns((string text, string? _) => CleanSegment(text));
        _shortStringHelper = mock.Object;
    }

    [Test]
    public void EnsureUniqueUrlSegment_Returns_Name_When_No_Collision()
    {
        SimilarNodeName[] siblings =
        {
            new() { Id = 1, Name = "Other" },
        };

        var result = DocumentRepository.EnsureUniqueUrlSegment("Title", 0, siblings, _shortStringHelper);

        Assert.AreEqual("Title", result);
    }

    [Test]
    public void EnsureUniqueUrlSegment_Appends_Suffix_When_Segments_Collide()
    {
        // "Title" and "Title." both produce segment "title"
        SimilarNodeName[] siblings =
        {
            new() { Id = 1, Name = "Title" },
        };

        var result = DocumentRepository.EnsureUniqueUrlSegment("Title.", 0, siblings, _shortStringHelper);

        Assert.AreEqual("Title. (1)", result);
    }

    [Test]
    public void EnsureUniqueUrlSegment_Three_Way_Collision_Increments_Suffix()
    {
        // "Title", "Title." and "Title!" all produce segment "title"
        SimilarNodeName[] siblings =
        {
            new() { Id = 1, Name = "Title" },
            new() { Id = 2, Name = "Title." },
        };

        var result = DocumentRepository.EnsureUniqueUrlSegment("Title!", 0, siblings, _shortStringHelper);

        // "Title! (1)" → segment "title-1" which doesn't collide with "title" or "title"
        Assert.AreEqual("Title! (1)", result);
    }

    [Test]
    public void EnsureUniqueUrlSegment_Skips_Suffix_When_It_Also_Collides()
    {
        // Force (1) suffix to also collide by having a sibling whose segment matches it.
        SimilarNodeName[] siblings =
        {
            new() { Id = 1, Name = "Title" },
            new() { Id = 2, Name = "Title 1" }, // produces segment "title-1", same as "Title. (1)"
        };

        var result = DocumentRepository.EnsureUniqueUrlSegment("Title.", 0, siblings, _shortStringHelper);

        Assert.AreEqual("Title. (2)", result);
    }

    [Test]
    public void EnsureUniqueUrlSegment_Excludes_Self_From_Collision_Check()
    {
        SimilarNodeName[] siblings =
        {
            new() { Id = 5, Name = "Title" },
        };

        // Node 5 checking its own name — should not collide with itself.
        var result = DocumentRepository.EnsureUniqueUrlSegment("Title", 5, siblings, _shortStringHelper);

        Assert.AreEqual("Title", result);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public void EnsureUniqueUrlSegment_Returns_Null_Or_Whitespace_Name_Unchanged(string? nodeName)
    {
        SimilarNodeName[] siblings =
        {
            new() { Id = 1, Name = "Title" },
        };

        var result = DocumentRepository.EnsureUniqueUrlSegment(nodeName, 0, siblings, _shortStringHelper);

        Assert.AreEqual(nodeName, result);
    }

    [Test]
    public void EnsureUniqueUrlSegment_Returns_Name_When_Segment_Is_Empty_After_Cleaning()
    {
        // Name consists entirely of characters stripped by the cleaner.
        var result = DocumentRepository.EnsureUniqueUrlSegment("...", 0, Array.Empty<SimilarNodeName>(), _shortStringHelper);

        Assert.AreEqual("...", result);
    }

    [Test]
    public void EnsureUniqueUrlSegment_Collision_Check_Is_Case_Insensitive()
    {
        SimilarNodeName[] siblings =
        {
            new() { Id = 1, Name = "TITLE" },
        };

        var result = DocumentRepository.EnsureUniqueUrlSegment("title.", 0, siblings, _shortStringHelper);

        Assert.AreEqual("title. (1)", result);
    }

    [Test]
    public void EnsureUniqueUrlSegment_Skips_Siblings_With_Null_Or_Whitespace_Names()
    {
        SimilarNodeName[] siblings =
        {
            new() { Id = 1, Name = null },
            new() { Id = 2, Name = string.Empty },
            new() { Id = 3, Name = "   " },
        };

        var result = DocumentRepository.EnsureUniqueUrlSegment("Title", 0, siblings, _shortStringHelper);

        Assert.AreEqual("Title", result);
    }

    [Test]
    public void EnsureUniqueUrlSegment_Passes_Culture_To_ShortStringHelper()
    {
        var mock = new Mock<IShortStringHelper>();
        mock.Setup(x => x.CleanStringForUrlSegment(It.IsAny<string>(), "da-DK"))
            .Returns((string text, string _) => CleanSegment(text));

        SimilarNodeName[] siblings =
        {
            new() { Id = 1, Name = "Title" },
        };

        var result = DocumentRepository.EnsureUniqueUrlSegment("Title.", 0, siblings, mock.Object, "da-DK");

        Assert.AreEqual("Title. (1)", result);
        mock.Verify(x => x.CleanStringForUrlSegment(It.IsAny<string>(), "da-DK"), Times.AtLeastOnce);
    }

    /// <summary>
    /// Simplified URL segment cleaner that mimics DefaultShortStringHelper behavior:
    /// lowercase, keep letters/digits/underscores, replace spaces with hyphens, strip the rest.
    /// </summary>
    private static string CleanSegment(string text)
    {
        var chars = new List<char>();
        foreach (var c in text.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                chars.Add(c);
            }
            else if (c == ' ' || c == '-')
            {
                if (chars.Count > 0 && chars[^1] != '-')
                {
                    chars.Add('-');
                }
            }
        }

        // Trim trailing dashes.
        while (chars.Count > 0 && chars[^1] == '-')
        {
            chars.RemoveAt(chars.Count - 1);
        }

        return new string(chars.ToArray());
    }
}
