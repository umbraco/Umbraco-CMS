using NUnit.Framework;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Search.Core.Services.ExternalMemberIndexing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Search.Core.Services;

[TestFixture]
public class ExternalMemberProfileDataIndexFieldsTests
{
    [Test]
    public void Create_Indexes_Strings_As_Texts()
    {
        IndexField field = ExternalMemberProfileDataIndexFields.Create("""{"favouriteColour":"Turquoise"}""", []).Single();

        Assert.Multiple(() =>
        {
            Assert.That(field.FieldName, Is.EqualTo("favouriteColour"));
            Assert.That(field.Value.Texts, Is.EqualTo(new[] { "Turquoise" }));
            Assert.That(field.Culture, Is.Null);
            Assert.That(field.Segment, Is.Null);
        });
    }

    [Test]
    public void Create_Indexes_Numbers_And_Booleans()
    {
        IndexField[] fields = ExternalMemberProfileDataIndexFields
            .Create("""{"count":42,"ratio":1.5,"huge":3000000000,"active":true,"archived":false}""", [])
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(Single(fields, "count").Value.Integers, Is.EqualTo(new[] { 42 }));
            Assert.That(Single(fields, "ratio").Value.Decimals, Is.EqualTo(new[] { 1.5m }));
            Assert.That(Single(fields, "huge").Value.Decimals, Is.EqualTo(new[] { 3000000000m }));
            Assert.That(Single(fields, "active").Value.Integers, Is.EqualTo(new[] { 1 }));
            Assert.That(Single(fields, "archived").Value.Integers, Is.EqualTo(new[] { 0 }));
        });
    }

    [Test]
    public void Create_Skips_Values_That_Cannot_Be_Indexed()
    {
        IEnumerable<IndexField> fields = ExternalMemberProfileDataIndexFields.Create(
            """{"empty":"","nothing":null,"list":["a","b"],"nested":{"a":"b"}}""",
            []);

        Assert.That(fields, Is.Empty);
    }

    [Test]
    public void Create_Does_Not_Overwrite_Reserved_Field_Names()
    {
        IndexField[] fields = ExternalMemberProfileDataIndexFields
            .Create("""{"Umb_Email":"spoofed@local","umb_name":"Spoofed","petName":"Biscuit"}""", ["Umb_Email", "Umb_Name"])
            .ToArray();

        Assert.That(fields.Select(field => field.FieldName), Is.EqualTo(new[] { "petName" }));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    [TestCase("not json")]
    [TestCase("""["Turquoise"]""")]
    [TestCase("\"Turquoise\"")]
    public void Create_Yields_No_Fields_For_Missing_Or_Non_Object_Profile_Data(string? profileData)
        => Assert.That(ExternalMemberProfileDataIndexFields.Create(profileData, []), Is.Empty);

    private static IndexField Single(IEnumerable<IndexField> fields, string fieldName)
        => fields.Single(field => field.FieldName == fieldName);
}
