using NUnit.Framework;
using Umbraco.Cms.Core.PropertyEditors.Patching;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.PropertyEditors.Patching;

[TestFixture]
public class PatchPathParserTests
{
    [Test]
    public void Parse_SimpleProperty_ReturnsPropertySegment()
    {
        var segments = PatchPathParser.Parse("/name");

        Assert.That(segments, Has.Length.EqualTo(1));
        Assert.That(segments[0], Is.InstanceOf<PropertySegment>());
        Assert.That(((PropertySegment)segments[0]).Name, Is.EqualTo("name"));
    }

    [Test]
    public void Parse_TwoProperties_ReturnsTwoPropertySegments()
    {
        var segments = PatchPathParser.Parse("/variants/name");

        Assert.That(segments, Has.Length.EqualTo(2));
        Assert.That(((PropertySegment)segments[0]).Name, Is.EqualTo("variants"));
        Assert.That(((PropertySegment)segments[1]).Name, Is.EqualTo("name"));
    }

    [Test]
    public void Parse_PropertyWithFilter_ReturnsPropertyAndFilterSegments()
    {
        var segments = PatchPathParser.Parse("/variants[culture=en-US,segment=null]/name");

        Assert.That(segments, Has.Length.EqualTo(3));
        Assert.That(segments[0], Is.InstanceOf<PropertySegment>());
        Assert.That(((PropertySegment)segments[0]).Name, Is.EqualTo("variants"));

        Assert.That(segments[1], Is.InstanceOf<FilterSegment>());
        var filter = (FilterSegment)segments[1];
        Assert.That(filter.Conditions, Has.Length.EqualTo(2));
        Assert.That(filter.Conditions[0].Key, Is.EqualTo("culture"));
        Assert.That(filter.Conditions[0].Value, Is.EqualTo("en-US"));
        Assert.That(filter.Conditions[1].Key, Is.EqualTo("segment"));
        Assert.That(filter.Conditions[1].Value, Is.Null);

        Assert.That(segments[2], Is.InstanceOf<PropertySegment>());
        Assert.That(((PropertySegment)segments[2]).Name, Is.EqualTo("name"));
    }

    [Test]
    public void Parse_ValuesPath_ReturnsCorrectSegments()
    {
        var segments = PatchPathParser.Parse("/values[alias=title,culture=en-US,segment=null]/value");

        Assert.That(segments, Has.Length.EqualTo(3));

        Assert.That(((PropertySegment)segments[0]).Name, Is.EqualTo("values"));

        var filter = (FilterSegment)segments[1];
        Assert.That(filter.Conditions, Has.Length.EqualTo(3));
        Assert.That(filter.Conditions[0], Is.EqualTo(new FilterCondition("alias", "title")));
        Assert.That(filter.Conditions[1], Is.EqualTo(new FilterCondition("culture", "en-US")));
        Assert.That(filter.Conditions[2], Is.EqualTo(new FilterCondition("segment", null)));

        Assert.That(((PropertySegment)segments[2]).Name, Is.EqualTo("value"));
    }

    [Test]
    public void Parse_NestedBlockListPath_ReturnsCorrectSegments()
    {
        var path = "/values[alias=contentBlocks,culture=null,segment=null]/value/contentData[key=block-2]/values[alias=headline]/value";
        var segments = PatchPathParser.Parse(path);

        Assert.That(segments, Has.Length.EqualTo(8));
        Assert.That(((PropertySegment)segments[0]).Name, Is.EqualTo("values"));
        Assert.That(segments[1], Is.InstanceOf<FilterSegment>());
        Assert.That(((PropertySegment)segments[2]).Name, Is.EqualTo("value"));
        Assert.That(((PropertySegment)segments[3]).Name, Is.EqualTo("contentData"));
        Assert.That(segments[4], Is.InstanceOf<FilterSegment>());
        Assert.That(((PropertySegment)segments[5]).Name, Is.EqualTo("values"));
        Assert.That(segments[6], Is.InstanceOf<FilterSegment>());
        Assert.That(((PropertySegment)segments[7]).Name, Is.EqualTo("value"));
    }

    [Test]
    public void Parse_IndexSegment_ReturnsIndexSegment()
    {
        var segments = PatchPathParser.Parse("/contentData/2/values");

        Assert.That(segments, Has.Length.EqualTo(3));
        Assert.That(segments[0], Is.InstanceOf<PropertySegment>());
        Assert.That(segments[1], Is.InstanceOf<IndexSegment>());
        Assert.That(((IndexSegment)segments[1]).Index, Is.EqualTo(2));
        Assert.That(segments[2], Is.InstanceOf<PropertySegment>());
    }

    [Test]
    public void Parse_AppendSegment_ReturnsAppendSegment()
    {
        var segments = PatchPathParser.Parse("/contentData/-");

        Assert.That(segments, Has.Length.EqualTo(2));
        Assert.That(segments[0], Is.InstanceOf<PropertySegment>());
        Assert.That(segments[1], Is.InstanceOf<AppendSegment>());
    }

    [Test]
    public void Parse_EmptyString_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => PatchPathParser.Parse(string.Empty));
    }

    [Test]
    public void Parse_NoLeadingSlash_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => PatchPathParser.Parse("name"));
    }

    [Test]
    public void Parse_UnclosedBracket_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => PatchPathParser.Parse("/values[alias=title"));
    }

    [Test]
    public void Parse_EmptyFilter_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => PatchPathParser.Parse("/values[]"));
    }

    [Test]
    public void Parse_FilterWithoutEquals_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => PatchPathParser.Parse("/values[alias]"));
    }

    [Test]
    public void IsValid_ValidSimplePath_ReturnsTrue()
    {
        Assert.That(PatchPathParser.IsValid("/name"), Is.True);
    }

    [Test]
    public void IsValid_ValidFilterPath_ReturnsTrue()
    {
        Assert.That(PatchPathParser.IsValid("/values[alias=title,culture=en-US,segment=null]/value"), Is.True);
    }

    [Test]
    public void IsValid_ValidAppendPath_ReturnsTrue()
    {
        Assert.That(PatchPathParser.IsValid("/contentData/-"), Is.True);
    }

    [Test]
    public void IsValid_EmptyString_ReturnsFalse()
    {
        Assert.That(PatchPathParser.IsValid(string.Empty), Is.False);
    }

    [Test]
    public void IsValid_InvalidSyntax_ReturnsFalse()
    {
        Assert.That(PatchPathParser.IsValid("/values[alias=title"), Is.False);
    }

    [Test]
    public void ExtractCultures_PathWithCulture_ReturnsCulture()
    {
        var cultures = PatchPathParser.ExtractCultures("/values[alias=title,culture=en-US,segment=null]/value");

        Assert.That(cultures, Has.Count.EqualTo(1));
        Assert.That(cultures, Does.Contain("en-US"));
    }

    [Test]
    public void ExtractCultures_PathWithNullCulture_ReturnsEmpty()
    {
        var cultures = PatchPathParser.ExtractCultures("/values[alias=title,culture=null,segment=null]/value");

        Assert.That(cultures, Is.Empty);
    }

    [Test]
    public void ExtractCultures_PathWithoutCulture_ReturnsEmpty()
    {
        var cultures = PatchPathParser.ExtractCultures("/values[alias=title]/value");

        Assert.That(cultures, Is.Empty);
    }

    [Test]
    public void ExtractCultures_InvalidPath_ReturnsEmpty()
    {
        var cultures = PatchPathParser.ExtractCultures(string.Empty);

        Assert.That(cultures, Is.Empty);
    }

    [Test]
    public void ExtractSegments_PathWithSegment_ReturnsSegment()
    {
        var segments = PatchPathParser.ExtractSegments("/values[alias=price,culture=en-US,segment=premium]/value");

        Assert.That(segments, Has.Count.EqualTo(1));
        Assert.That(segments, Does.Contain("premium"));
    }

    [Test]
    public void ExtractSegments_PathWithNullSegment_ReturnsEmpty()
    {
        var segments = PatchPathParser.ExtractSegments("/values[alias=price,segment=null]/value");

        Assert.That(segments, Is.Empty);
    }

    [Test]
    public void ExtractSegments_PathWithoutSegment_ReturnsEmpty()
    {
        var segments = PatchPathParser.ExtractSegments("/values[alias=title]/value");

        Assert.That(segments, Is.Empty);
    }

    [Test]
    public void TargetsInvariantCulture_NullCulture_ReturnsTrue()
    {
        Assert.That(PatchPathParser.TargetsInvariantCulture("/values[alias=title,culture=null]/value"), Is.True);
    }

    [Test]
    public void TargetsInvariantCulture_SpecificCulture_ReturnsFalse()
    {
        Assert.That(PatchPathParser.TargetsInvariantCulture("/values[alias=title,culture=en-US]/value"), Is.False);
    }

    [Test]
    public void TargetsInvariantCulture_NoCulture_ReturnsFalse()
    {
        Assert.That(PatchPathParser.TargetsInvariantCulture("/values[alias=title]/value"), Is.False);
    }

    [Test]
    public void TargetsInvariantCulture_InvalidPath_ReturnsFalse()
    {
        Assert.That(PatchPathParser.TargetsInvariantCulture(string.Empty), Is.False);
    }

    // RFC 6901 escape sequence tests

    [Test]
    public void Parse_Tilde1EscapeSequence_DecodesToSlash()
    {
        var segments = PatchPathParser.Parse("/a~1b");

        Assert.That(segments, Has.Length.EqualTo(1));
        Assert.That(segments[0], Is.InstanceOf<PropertySegment>());
        Assert.That(((PropertySegment)segments[0]).Name, Is.EqualTo("a/b"));
    }

    [Test]
    public void Parse_Tilde0EscapeSequence_DecodesToTilde()
    {
        var segments = PatchPathParser.Parse("/a~0b");

        Assert.That(segments, Has.Length.EqualTo(1));
        Assert.That(segments[0], Is.InstanceOf<PropertySegment>());
        Assert.That(((PropertySegment)segments[0]).Name, Is.EqualTo("a~b"));
    }

    [Test]
    public void Parse_Tilde01Combined_DecodesCorrectly()
    {
        // ~01 should decode to ~1 (not /), because ~0 decodes to ~ and the trailing 1 stays.
        // The implementation decodes ~1 first (to /), then ~0 (to ~).
        // So ~01 → after ~1 pass: ~01 (no match) → after ~0 pass: ~1? No...
        // Actually: "~01" → Replace("~1", "/") has no match → Replace("~0", "~") → "~1"
        var segments = PatchPathParser.Parse("/~01");

        Assert.That(segments, Has.Length.EqualTo(1));
        Assert.That(segments[0], Is.InstanceOf<PropertySegment>());
        Assert.That(((PropertySegment)segments[0]).Name, Is.EqualTo("~1"));
    }

    [Test]
    public void Parse_NoEscapeSequences_PropertyNameUnchanged()
    {
        var segments = PatchPathParser.Parse("/normalProperty");

        Assert.That(segments, Has.Length.EqualTo(1));
        Assert.That(((PropertySegment)segments[0]).Name, Is.EqualTo("normalProperty"));
    }

    [Test]
    public void Parse_EscapeSequenceInMultiSegmentPath_DecodesOnlyAffectedSegment()
    {
        var segments = PatchPathParser.Parse("/foo/bar~1baz/qux");

        Assert.That(segments, Has.Length.EqualTo(3));
        Assert.That(((PropertySegment)segments[0]).Name, Is.EqualTo("foo"));
        Assert.That(((PropertySegment)segments[1]).Name, Is.EqualTo("bar/baz"));
        Assert.That(((PropertySegment)segments[2]).Name, Is.EqualTo("qux"));
    }

    [Test]
    public void IsValid_PathWithEscapeSequence_ReturnsTrue()
    {
        Assert.That(PatchPathParser.IsValid("/foo~1bar"), Is.True);
        Assert.That(PatchPathParser.IsValid("/foo~0bar"), Is.True);
    }
}
